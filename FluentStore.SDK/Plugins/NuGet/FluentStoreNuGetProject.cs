﻿using FluentStore.SDK.Helpers;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.NuGet;

public class FluentStoreNuGetProject : NuGetProject
{
    const string StatusFileName = "status.tsv";

    private static readonly SourceRepository _officialSource =
        Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

    private readonly PluginStatusRecord _entries;
    private readonly string _statusFilePath;
    private readonly SourceCacheContext _cache = new();

    public string PluginRoot { get; }

    public string Name { get; }

    public NuGetFramework TargetFramework { get; }

    public IReadOnlyDictionary<string, PluginEntry> Entries => _entries;

    public IReadOnlySet<PackageIdentity> IgnoredDependencies { get; set; }

    public List<SourceRepository> Repositories { get; } = [_officialSource];

    public static NuGetVersion CurrentSdkVersion { get; }

    public static VersionRange SupportedSdkRange { get; }

    static FluentStoreNuGetProject()
    {
        CurrentSdkVersion = new(typeof(PluginLoader).Assembly.GetName().Version!, "beta");
        NuGetVersion nextBreakingSdkVersion = new(0, CurrentSdkVersion.Minor + 1, 0, "alpha");

        SupportedSdkRange = VersionRange.Parse($"[{CurrentSdkVersion.Major}.{CurrentSdkVersion.Minor}.*-*, {nextBreakingSdkVersion})");
    }

    public FluentStoreNuGetProject(string pluginRoot, NuGetFramework targetFramework, string name = "FluentStore")
    {
        PluginRoot = pluginRoot;
        Name = name;
        TargetFramework = targetFramework;

        _statusFilePath = Path.Combine(PluginRoot, StatusFileName);

        if (!File.Exists(_statusFilePath))
        {
            File.Create(_statusFilePath).Dispose();
            _entries = [];
        }
        else
        {
            _entries = PluginStatusRecord.Read(_statusFilePath);
        }
    }

    public void AddFeeds(IEnumerable<string> feeds)
    {
        foreach (var feed in feeds)
        {
            var source = CreateAbstractStorageSourceRepository(feed);
            Repositories.Add(source);
        }
    }

    public async Task<DownloadResourceResult> DownloadPackageAsync(string packageId, VersionRange versionRange = null, CancellationToken token = default)
    {
        versionRange ??= SupportedSdkRange;

        // Search all available feeds for compatible versions
        foreach (var repo in Repositories)
        {
            try
            {
                var resource = await repo.GetResourceAsync<FindPackageByIdResource>(token);

                var allDepVersions = await resource.GetAllVersionsAsync(packageId, _cache, NullLogger.Instance, token);
                var version = versionRange.FindBestMatch(allDepVersions);
                if (version is null)
                    continue;

                MemoryStream nupkgStream = new();
                await resource.CopyNupkgToStreamAsync(packageId, version,
                    nupkgStream, _cache, NullLogger.Instance, token);

                PackageArchiveReader nupkgReader = new(nupkgStream);
                DownloadResourceResult resourceResult = new(nupkgStream, nupkgReader, repo.PackageSource.Source);
                return resourceResult;
            }
            catch
            {
                continue;
            }
        }

        throw new Exception($"Failed to find {packageId} in any NuGet feed.");
    }

    public override async Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token = default)
    {
        var pluginFolder = Path.Combine(PluginRoot, packageIdentity.Id);
        PluginInstallStatus status = PluginInstallStatus.Completed;
        NuGetFramework tfm = NuGetFramework.UnsupportedFramework;
        VersionRange sdkVersion = VersionRange.None;

        if (nuGetProjectContext?.ActionType == NuGetActionType.Reinstall)
        {
            var uninstallStatus = await UninstallPackageAsync(packageIdentity.Id, nuGetProjectContext, token);
            if (uninstallStatus is PluginInstallStatus.AppRestartRequired or PluginInstallStatus.SystemRestartRequired)
            {
                // App will need to restart, make sure the package is
                // in the plugin folder so we can pick it up next time.
                status = uninstallStatus;

                string pluginFilePath = Path.Combine(PluginRoot, $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg");
                await downloadResourceResult.PackageReader.CopyNupkgAsync(pluginFilePath, token);
            }
        }

        if (status == PluginInstallStatus.Completed)
        {
            (status, tfm, sdkVersion) = await InstallPackageCoreAsync(packageIdentity, downloadResourceResult,
                nuGetProjectContext, pluginFolder, true, token);
        }

        // Add plugin to list of installed packages
        _entries[packageIdentity.Id] = new(packageIdentity, tfm, status, PluginInstallStatus.NoAction, sdkVersion);
        await FlushAsync(token);

        return status.IsAtLeast(PluginInstallStatus.AppRestartRequired);
    }

    public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token = default)
    {
        var status = await UninstallPackageAsync(packageIdentity.Id, nuGetProjectContext, token);
        return status.IsAtLeast(PluginInstallStatus.NoAction);
    }

    public async Task<PluginInstallStatus> UninstallPackageAsync(string packageId, INuGetProjectContext nuGetProjectContext, CancellationToken token = default)
    {
        PluginInstallStatus uninstallStatus = PluginInstallStatus.Failed;

        try
        {
            // Delete all plugin files
            var pluginFolder = Path.Combine(PluginRoot, packageId);
            Directory.Delete(pluginFolder, true);
            uninstallStatus = PluginInstallStatus.Completed;
        }
        catch (UnauthorizedAccessException)
        {
            uninstallStatus = PluginInstallStatus.AppRestartRequired;
        }
        catch (DirectoryNotFoundException)
        {
            uninstallStatus = PluginInstallStatus.NoAction;
        }
        catch (IOException)
        {
            uninstallStatus = PluginInstallStatus.AppRestartRequired;
        }
        catch (Exception ex)
        {
            nuGetProjectContext?.ReportError(ex.ToString());
            throw;
        }

        if (uninstallStatus is PluginInstallStatus.Completed)
        {
            // Remove plugin from list of installed packages
            _entries.Remove(packageId);
            await FlushAsync(token);
        }
        else if (_entries.TryGetValue(packageId, out var entry))
        {
            entry.UninstallStatus = uninstallStatus;
            _entries[packageId] = entry;
            await FlushAsync(token);
        }

        return uninstallStatus;
    }

    public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token = default)
    {
        return _entries.Values
            .Where(e => e.InstallStatus == PluginInstallStatus.Completed)
            .Select(e => e.ToPackageReference());
    }

    public async Task FlushAsync(CancellationToken token = default)
    {
        await File.WriteAllLinesAsync(_statusFilePath, _entries.Values.Select(e => e.ToString()), token);
    }

    public static SourceRepository CreateAbstractStorageSourceRepository(string url)
    {
        var providers = new Lazy<INuGetResourceProvider>[]
        {
            new(() => new AbstractStoragePackageSearchResourceV3Provider()),
            new(() => new AbstractStorageFindPackageByIdResourceProvider()),
            new(() => new AbstractStorageResourceProvider()),
            new(() => new AbstractStorageServiceIndexResourceV3Provider()),
        }.Concat(Repository.Provider.GetCoreV3());

        var source = Repository.CreateSource(providers, url);
        source.PackageSource.ProtocolVersion = 3;
        return source;
    }

    public (NuGetFramework tfm, IEnumerable<PackageDependency> deps, PackageDependency sdkDep) GetCompatibleDependencies(PackageReaderBase reader)
    {
        var id = reader.GetIdentity().Id;
        FrameworkReducer tfmReducer = new();

        // Get target framework
        var tfm = tfmReducer.GetNearest(TargetFramework, reader.GetSupportedFrameworks());
        if (tfm is null)
        {
            // For some packages, the only supported framework that's compatible is .NET Standard,
            // which doesn't seem to be included in reader.GetSupportedFrameworks().
            var refFrameworks = reader.GetItems("ref").Select(g => g.TargetFramework);
            tfm = tfmReducer.GetNearest(TargetFramework, refFrameworks);

            if (tfm is null)
                throw new Exception($"{id} does not support {TargetFramework}");
        }

        // Get list of dependencies
        var deps = GetDependencies(reader.GetPackageDependencies(), tfm);

        // Ensure compatible SDK version
        var sdkDep = deps?.FirstOrDefault(d => d.Id == "FluentStore.SDK");
        if (sdkDep is not null && deps is not null && !sdkDep.VersionRange.Satisfies(CurrentSdkVersion))
            throw new PluginSdkNotSupportedException(id, sdkDep.VersionRange);

        return (tfm, deps, sdkDep);
    }

    public bool CheckCompatibility(NuGetFramework tfm, VersionRange sdkVersion, IFrameworkCompatibilityProvider compat = null)
    {
        compat ??= DefaultCompatibilityProvider.Instance;
        return compat.IsCompatible(tfm, TargetFramework) && sdkVersion.Satisfies(CurrentSdkVersion);
    }

    public IEnumerable<PackageDependency> GetDependencies(IEnumerable<PackageDependencyGroup> dependencyGroups)
    {
        FrameworkReducer tfmReducer = new();
        var supportedTfms = dependencyGroups.Select(s => s.TargetFramework);
        var tfm = tfmReducer.GetNearest(TargetFramework, supportedTfms);
        return GetDependencies(dependencyGroups, tfm);
    }

    public static IEnumerable<PackageDependency> GetDependencies(IEnumerable<PackageDependencyGroup> dependencyGroups, NuGetFramework tfm)
    {
        var group = dependencyGroups
            .FirstOrDefault(IsGroupCompatibleFilter(tfm))
            as PackageDependencyGroup;
        return group?.Packages;
    }

    private async Task<(PluginInstallStatus status, NuGetFramework tfm, VersionRange sdkVersion)> InstallPackageCoreAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, string pluginFolder, bool isPlugin, CancellationToken token = default)
    {
        var status = PluginInstallStatus.Completed;
        var reader = downloadResourceResult.PackageReader;
        var nameProvider = DefaultFrameworkNameProvider.Instance;
        FrameworkReducer tfmReducer = new();

        var (tfm, dependencies, sdkDependency) = GetCompatibleDependencies(reader);
        
        if (sdkDependency is null && isPlugin)
            throw new Exception($"{packageIdentity.Id} is not a Fluent Store plugin.");

        // Check if this package is newer than an already installed one
        bool attemptInstall = true;
        if (isPlugin && Entries.TryGetValue(packageIdentity.Id, out var existingEntry))
        {
            if (reader.NuspecReader.GetVersion() > existingEntry.Version)
            {
                // This is a newer version; uninstall the old one
                if (!await UninstallPackageAsync(existingEntry.ToPackageIdentity(), nuGetProjectContext, token))
                {
                    status = PluginInstallStatus.AppRestartRequired;
                    attemptInstall = false;
                }
            }
            else if (existingEntry.InstallStatus.IsAtLeast(PluginInstallStatus.AppRestartRequired))
            {
                // This plugin has already been installed successfully
                status = PluginInstallStatus.NoAction;
                attemptInstall = false;
            }
        }

        if (attemptInstall)
        {
            Directory.CreateDirectory(pluginFolder);

            // Copy DLLs
            List<string> includedDlls = new();
            nuGetProjectContext.Log(MessageLevel.Debug, "Copying files for {0}...", packageIdentity.Id);

            // Locate primary plugin DLL and direct deps
            var libItems = GetFirstCompatibleItems(reader.GetLibItems(), tfm);
            if (libItems is not null)
            {
                reader.CopyFiles(pluginFolder, libItems.Append(reader.GetNuspecFile()), (src, _, stream) =>
                {
                    string dst;

                    if (Path.EndsInDirectorySeparator(src))
                    {
                        dst = Path.Combine(pluginFolder, src);
                        Directory.CreateDirectory(dst);
                    }
                    else
                    {
                        var fileName = Path.GetFileName(src);
                        dst = Path.Combine(pluginFolder, Path.GetFileName(src));
                    
                        using var file = File.OpenWrite(dst);
                        stream.CopyTo(file);
                        includedDlls.Add(Path.GetFileNameWithoutExtension(src));
                    }

                    return dst;
                }, NullLogger.Instance, token);
            }

            // Copy any references
            var refItems = GetFirstCompatibleItems(reader.GetItems("ref"), tfm);
            if (refItems is not null)
            {
                reader.CopyFiles(pluginFolder, refItems, CopyPackageContent, NullLogger.Instance, token);
            }

            // Copy any content/resources
            var contentItems = GetFirstCompatibleItems(reader.GetContentItems(), tfm);
            if (contentItems is not null)
            {
                reader.CopyFiles(pluginFolder, contentItems, CopyPackageContent, NullLogger.Instance, token);
            }

            if (dependencies is not null)
            {
                nuGetProjectContext.Log(MessageLevel.Debug, "Installing dependencies for {0}", packageIdentity.Id);

                // Figure out which dependencies don't need to be downloaded
                var installed = IgnoredDependencies
                    .Concat(Entries.Select(p => p.Value.ToPackageIdentity()))
                    .Concat(includedDlls.Select(d => new PackageIdentity(d, new(0, 0, 0))))
                    .ToList();

                // Download and install any NuGet references
                foreach (var dependency in dependencies.Where(d => !IsDependencyAlreadyFulfilled(installed, d)))
                {
                    nuGetProjectContext.Log(MessageLevel.Debug, "Downloading dependency {0}", dependency);
                    var downloadResult = await DownloadPackageAsync(dependency.Id, dependency.VersionRange, token: token);

                    nuGetProjectContext.Log(MessageLevel.Debug, "Installing dependency {0}", dependency);
                    var (depStatus, _, _) = await InstallPackageCoreAsync(downloadResult.PackageReader.GetIdentity(),
                        downloadResult, nuGetProjectContext, pluginFolder, false, token);

                    if (depStatus < status)
                        status = depStatus;
                }
            }
            else
            {

            }
        }

        return (status, tfm, sdkDependency?.VersionRange);
    }

    private static string CopyPackageContent(string src, string dst, Stream stream)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        using var file = File.OpenWrite(dst);
        stream.CopyTo(file);
        return dst;
    }

    private static bool IsDependencyAlreadyFulfilled(IEnumerable<PackageIdentity> installed, PackageDependency dep)
    {
        return installed.Any(package => package.Id == dep.Id && dep.VersionRange.Satisfies(package.Version));
    }

    private static IEnumerable<string> GetFirstCompatibleItems(IEnumerable<FrameworkSpecificGroup> groups, NuGetFramework tfm)
    {
        var isGroupCompatible = IsGroupCompatibleFilter(tfm);
        var frameworkGroups = groups.FirstOrDefault(isGroupCompatible) as FrameworkSpecificGroup;
        return frameworkGroups?.Items;
    }

    /// <summary>
    /// Tests whether a given framework is compatible with the current target.
    /// </summary>
    /// <param name="candidate"></param>
    /// <returns></returns>
    private static Func<IFrameworkSpecific, bool> IsGroupCompatibleFilter(NuGetFramework tfm)
    {
        return (IFrameworkSpecific candidate) =>
        {
            var compatibilityProvider = DefaultCompatibilityProvider.Instance;
            return compatibilityProvider.IsCompatible(tfm, candidate.TargetFramework);
        };
    }
}

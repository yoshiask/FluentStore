using FluentStore.SDK.Helpers;
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

namespace FluentStore.SDK.Plugins;

public class FluentStoreNuGetProject : NuGetProject
{
    private static readonly NuGetVersion _currentSdkVersion = new(typeof(PluginLoader).Assembly.GetName().Version!);
    const string StatusFileName = "status.tsv";

    private static readonly SourceRepository _officialSource =
        Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

    private readonly Dictionary<string, PluginEntry> _entries;
    private readonly string _statusFilePath;
    private readonly SourceCacheContext _cache = new();

    public string PluginRoot { get; }

    public string Name { get; }

    public NuGetFramework TargetFramework { get; }

    public IReadOnlyDictionary<string, PluginEntry> Entries => _entries;

    public IReadOnlySet<string> IgnoredDependencies { get; set; }

    public List<SourceRepository> Repositories { get; } = new() { _officialSource };

    public FluentStoreNuGetProject(string pluginRoot, NuGetFramework targetFramework, string name = "FluentStore")
    {
        PluginRoot = pluginRoot;
        Name = name;
        TargetFramework = targetFramework;

        _statusFilePath = Path.Combine(PluginRoot, StatusFileName);

        if (!File.Exists(_statusFilePath))
        {
            File.Create(_statusFilePath).Dispose();
            _entries = new();
        }
        else
        {
            var lines = File.ReadAllLines(_statusFilePath);
            _entries = new(lines.Length);

            foreach (var line in lines)
            {
                var entry = PluginEntry.Parse(line);
                _entries.Add(entry.Id, entry);
            }
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

    public async Task<DownloadResourceResult> DownloadPackageAsync(string packageId, VersionRange versionRange,
        CancellationToken token = default)
    {
        NuGetVersion[] allDepVersions = null;
        SourceRepository repo = null;
        FindPackageByIdResource resource = null;
        
        // Search all available feeds for compatible versions
        foreach (var r in Repositories)
        {
            try
            {
                repo = r;
                resource = await repo.GetResourceAsync<FindPackageByIdResource>(token);

                allDepVersions =
                    (await resource.GetAllVersionsAsync(packageId, _cache, NullLogger.Instance, token))
                    .ToArray();
                if (!allDepVersions.Any())
                    continue;

                MemoryStream depStream = new();
                await resource.CopyNupkgToStreamAsync(packageId, versionRange.FindBestMatch(allDepVersions),
                    depStream, _cache, NullLogger.Instance, token);

                PackageArchiveReader depReader = new(depStream);
                DownloadResourceResult resourceResult = new(depStream, depReader, repo.PackageSource.Source);
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

        if (nuGetProjectContext?.ActionType == NuGetActionType.Reinstall)
        {
            var uninstalled = await UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);
            if (!uninstalled)
            {
                // App will need to restart, make sure the package is
                // in the plugin folder so we can pick it up next time.
                status = PluginInstallStatus.AppRestartRequired;

                string pluginFilePath = Path.Combine(PluginRoot, $"{packageIdentity}.nupkg");
                await downloadResourceResult.PackageReader.CopyNupkgAsync(pluginFilePath, token);
            }
        }

        if (status == PluginInstallStatus.Completed)
        {
            (status, tfm) = await InstallPackageCoreAsync(packageIdentity, downloadResourceResult,
                nuGetProjectContext, pluginFolder, true, token);
        }

        // Add plugin to list of installed packages
        _entries[packageIdentity.Id] = new(packageIdentity, tfm, status);
        await FlushAsync(token);

        return status.IsAtLeast(PluginInstallStatus.AppRestartRequired);
    }

    public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token = default)
    {
        try
        {
            // Delete all plugin files
            var pluginFolder = Path.Combine(PluginRoot, packageIdentity.Id);
            Directory.Delete(pluginFolder, true);
        }
        catch (DirectoryNotFoundException) { }
        catch (Exception ex)
        {
            nuGetProjectContext?.ReportError(ex.ToString());
            return false;
        }

        // Remove plugin from list of installed packages
        _entries.Remove(packageIdentity.Id);
        await FlushAsync(token);

        return true;
    }

    public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token = default)
    {
        return _entries.Values
            .Where(e => e.Status == PluginInstallStatus.Completed)
            .Select(e => e.ToPackageReference());
    }

    public async Task FlushAsync(CancellationToken token = default)
    {
        await File.WriteAllLinesAsync(_statusFilePath, _entries.Values.Select(e => e.ToString()), token);
    }

    private async Task<(PluginInstallStatus status, NuGetFramework tfm)> InstallPackageCoreAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, string pluginFolder, bool isPlugin, CancellationToken token = default)
    {
        var status = PluginInstallStatus.Completed;
        var reader = downloadResourceResult.PackageReader;
        var nameProvider = DefaultFrameworkNameProvider.Instance;
        var compatibilityProvider = DefaultCompatibilityProvider.Instance;
        FrameworkReducer tfmReducer = new();

        // Get target framework
        var tfm = tfmReducer.GetNearest(TargetFramework, await reader.GetSupportedFrameworksAsync(token));
        if (tfm is null)
        {
            // For some packages, the only supported framework that's compatible is .NET Standard,
            // which doesn't seem to be included in reader.GetSupportedFrameworks().
            var refFrameworks = reader.GetItems("ref").Select(g => g.TargetFramework);
            tfm = tfmReducer.GetNearest(TargetFramework, refFrameworks);
            
            if (tfm is null)
                throw new Exception($"{packageIdentity.Id} does not support {TargetFramework}");
        }

        // Create method to test if a given framework is compatible with the current target
        bool IsGroupCompatible(IFrameworkSpecific candidate)
        {
            return compatibilityProvider.IsCompatible(tfm, candidate.TargetFramework);
        }

        // Get list of dependencies
        var dependencies = reader.NuspecReader.GetDependencyGroups().FirstOrDefault(IsGroupCompatible)?.Packages;

        if (isPlugin)
        {
            // Ensure compatible SDK version
            var sdkDep = dependencies?.FirstOrDefault(d => d.Id == "FluentStore.SDK");
            if (dependencies is not null && !sdkDep.VersionRange.Satisfies(_currentSdkVersion))
                throw new Exception($"{packageIdentity.Id} does not support Fluent Store SDK {_currentSdkVersion}: requires {sdkDep.VersionRange}");
        }

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
            else if (existingEntry.Status.IsAtLeast(PluginInstallStatus.AppRestartRequired))
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
            var libItems = reader.GetLibItems().FirstOrDefault(IsGroupCompatible)?.Items;
            if (libItems is not null)
            {
                reader.CopyFiles(pluginFolder, libItems.Append(reader.GetNuspecFile()), (src, _, stream) =>
                {
                    var dst = Path.Combine(pluginFolder, Path.GetFileName(src));
                    using var file = File.OpenWrite(dst);
                    stream.CopyTo(file);
                    includedDlls.Add(Path.GetFileNameWithoutExtension(src));
                    return dst;
                }, NullLogger.Instance, token);
            }

            // Copy any references
            var refItems = reader.GetItems("ref").FirstOrDefault(IsGroupCompatible)?.Items;
            if (refItems is not null)
            {
                reader.CopyFiles(pluginFolder, refItems, CopyPackageContent, NullLogger.Instance, token);
            }

            // Copy any content/resources
            var contentItems = reader.GetContentItems().FirstOrDefault(IsGroupCompatible)?.Items;
            if (contentItems is not null)
            {
                reader.CopyFiles(pluginFolder, contentItems, CopyPackageContent, NullLogger.Instance, token);
            }

            if (dependencies is not null)
            {
                nuGetProjectContext.Log(MessageLevel.Debug, "Installing dependencies for {0}", packageIdentity.Id);
                
                // Figure out which dependencies don't need to be downloaded
                var ignoredDeps = IgnoredDependencies
                    .Concat(Entries.Keys)
                    .Concat(includedDlls)
                    .ToHashSet();

                // Download and install any NuGet references
                foreach (var dependency in dependencies.Where(d => !ignoredDeps.Contains(d.Id)))
                {
                    nuGetProjectContext.Log(MessageLevel.Debug, "Downloading dependency {0}", dependency);
                    var downloadResult = await DownloadPackageAsync(dependency.Id, dependency.VersionRange, token);
                    
                    nuGetProjectContext.Log(MessageLevel.Debug, "Installing dependency {0}", dependency);
                    var (depStatus, _) = await InstallPackageCoreAsync(downloadResult.PackageReader.GetIdentity(),
                        downloadResult, nuGetProjectContext, pluginFolder, false, token);

                    if (depStatus < status)
                        status = depStatus;
                }
            }
            else
            {

            }
        }

        return (status, tfm);
    }

    private static string CopyPackageContent(string src, string dst, Stream stream)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dst));
        using var file = File.OpenWrite(dst);
        stream.CopyTo(file);
        return dst;
    }

    private static SourceRepository CreateAbstractStorageSourceRepository(string url)
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
}

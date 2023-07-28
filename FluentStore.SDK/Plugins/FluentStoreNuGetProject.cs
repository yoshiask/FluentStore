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

internal class FluentStoreNuGetProject : NuGetProject
{
    private static readonly NuGetVersion _currentSdkVersion = new(typeof(PluginLoader).Assembly.GetName().Version);
    private static readonly string[] IgnoredDependencies = new[] { "FluentStore.SDK" };
    const string StatusFileName = "status.csv";

    private readonly Dictionary<string, PluginEntry> _entries;
    private readonly string _statusFilePath;
    private readonly SourceCacheContext _cache = new();

    public string PluginRoot { get; }

    public string Name { get; }

    public NuGetFramework TargetFramework { get; }

    public IReadOnlyDictionary<string, PluginEntry> Entries => _entries;

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

    public override async Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
    {
        var pluginFolder = Path.Combine(PluginRoot, packageIdentity.Id);
        var (status, tfm) = await InstallPackageCoreAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, pluginFolder, true, token);

        if (!status.IsAtLeast(PluginInstallStatus.AppRestartRequired))
            return false;

        // Add plugin to list of installed packages
        _entries.Add(packageIdentity.Id, new(packageIdentity, tfm, status));
        await FlushAsync(token);
        return true;
    }

    public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
    {
        // Delete all plugin files
        Directory.Delete(packageIdentity.Id, true);

        // Remove plugin from list of installed packages
        _entries.Remove(packageIdentity.Id);
        await FlushAsync(token);
        return true;
    }

    public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
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
        var status = PluginInstallStatus.Failed;
        var reader = downloadResourceResult.PackageReader;

        // Get list of dependencies
        FrameworkReducer tfmReducer = new();
        var tfm = tfmReducer.GetNearest(TargetFramework, reader.GetSupportedFrameworks());
        var dependencies = reader.NuspecReader.GetDependencyGroups().FirstOrDefault(g => g.TargetFramework == tfm)?.Packages;

        if (isPlugin)
        {
            // Ensure compatible SDK version
            var sdkDep = dependencies?.FirstOrDefault(d => d.Id == "FluentStore.SDK");
            if (dependencies is not null && !sdkDep.VersionRange.Satisfies(_currentSdkVersion))
                throw new Exception($"{packageIdentity.Id} does not support Fluent Store SDK {_currentSdkVersion}: requires {sdkDep.VersionRange}");
        }

        // Locate primary plugin DLL and direct deps
        var items = reader.GetLibItems().FirstOrDefault(g => g.TargetFramework == tfm)?.Items
            ?? throw new Exception($"{packageIdentity.Id} does not support this version of Fluent Store.");

        // Check if this package is newer than an already installed one
        bool attemptInstall = true;
        if (isPlugin && Entries.TryGetValue(packageIdentity.Id, out var existingEntry))
        {
            if (reader.NuspecReader.GetVersion() > existingEntry.Version)
            {
                if (!await UninstallPackageAsync(existingEntry.GetPackageIdentity(), nuGetProjectContext, token))
                {
                    status = PluginInstallStatus.AppRestartRequired;
                    attemptInstall = false;
                }
            }
            else
            {
                status = PluginInstallStatus.NoAction;
                attemptInstall = false;
            }
        }

        if (attemptInstall)
        {
            Directory.CreateDirectory(pluginFolder);

            // Copy DLLs
            reader.CopyFiles(pluginFolder, items.Append(reader.GetNuspecFile()), (src, _, stream) =>
            {
                var dst = Path.Combine(pluginFolder, Path.GetFileName(src));
                using var file = File.OpenWrite(dst);
                stream.CopyTo(file);
                return dst;
            }, NullLogger.Instance, token);

            // Copy any content/resources
            var contentItems = reader.GetContentItems().FirstOrDefault(g => g.TargetFramework == tfm)?.Items;
            if (contentItems is not null)
            {
                reader.CopyFiles(pluginFolder, contentItems, (src, dst, stream) =>
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    using var file = File.OpenWrite(dst);
                    stream.CopyTo(file);
                    return dst;
                }, NullLogger.Instance, token);
            }

            // Download and install any NuGet references
            var ignoredDeps = IgnoredDependencies.Concat(Entries.Keys).ToArray();
            foreach (var dependency in dependencies.Where(d => !ignoredDeps.Contains(d.Id)))
            {
                SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
                FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                var allDepVersions = await resource.GetAllVersionsAsync(dependency.Id, _cache, NullLogger.Instance, token);

                using MemoryStream depStream = new();
                await resource.CopyNupkgToStreamAsync(dependency.Id, dependency.VersionRange.FindBestMatch(allDepVersions),
                    depStream, _cache, NullLogger.Instance, token);

                using PackageArchiveReader depReader = new(depStream);
                DownloadResourceResult resourceResult = new(depStream, depReader, repository.PackageSource.Source);
                await InstallPackageCoreAsync(depReader.GetIdentity(), resourceResult, nuGetProjectContext, pluginFolder, false, token);
            }

            status = PluginInstallStatus.Completed;
        }

        return (status, tfm);
    }
}

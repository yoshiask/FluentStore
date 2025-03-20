using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using OwlCore.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using System.Runtime.Loader;
using NuGet.Packaging.Core;
using FluentStore.SDK.Plugins.NuGet;
using NuGet.Protocol.Core.Types;

namespace FluentStore.SDK.Plugins
{
    public class PluginLoader : IAsyncInit
    {
        private const string FLUENTSTORE_FEED = "ipns://ipfs.askharoun.com/FluentStore/Plugins/NuGet/index.json";

        private static readonly NuGetFramework _targetFramework = NuGetFramework.Parse("net8.0-windows10.0.22621.0");
        private static readonly NuGetVersion _targetFrameworkVersion = new(_targetFramework.Version);
        private static readonly SourceRepository _fluentStoreRepo = FluentStoreNuGetProject.CreateAbstractStorageSourceRepository(FLUENTSTORE_FEED);

        private readonly HashSet<PackageIdentity> _loadedAssemblies =
        [
            // SDK reference
            new("NETStandard.Library", new NuGetVersion(2, 1, 0)),

            // Known .NET dependencies
            new("System.ComponentModel.Primitives", _targetFrameworkVersion),
            new("System.ComponentModel.TypeConverter", _targetFrameworkVersion),
            new("System.Diagnostics.DiagnosticSource", _targetFrameworkVersion),
            new("System.Dynamic.Runtime", _targetFrameworkVersion),
            new("System.Runtime.Serialization.Primitives", _targetFrameworkVersion),
            new("System.Text.Json", new(9, 0, 0)),
            new("Microsoft.CSharp", _targetFrameworkVersion),
            new("System.Reflection", _targetFrameworkVersion),
        ];

        private readonly ISettingsService _settings;
        private readonly PackageService _packageService;
        private readonly IPasswordVaultService _passwordVaultService;
        private readonly LoggerService _log;
        private readonly FluentStoreProjectContext _projCtx;
        private readonly Dictionary<string, PluginLoadResult> _loadedPlugins = [];

        public static SourceRepository FluentStoreRepo => _fluentStoreRepo;

        public bool IsInitialized { get; private set; }

        public FluentStoreNuGetProject Project { get; init; }

        public PluginLoader(ISettingsService settings, PackageService packageService, IPasswordVaultService passwordVaultService, LoggerService log)
        {
            _settings = settings;
            _packageService = packageService;
            _passwordVaultService = passwordVaultService;
            _log = log;
            _projCtx = new(settings.PluginDirectory, _log);

            Project = new(settings.PluginDirectory, _targetFramework);
            Project.Repositories.Add(FluentStoreRepo);
        }

        public async Task InitAsync(CancellationToken token = default)
        {
            if (IsInitialized)
                return;

            await Task.Run(() =>
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var assemblyInfo = assembly.GetName();
                    var assemblyName = assemblyInfo.Name;
                    if (assemblyName is not null)
                    {
                        PackageIdentity identity = new(assemblyName, new(assemblyInfo.Version));
                        _loadedAssemblies.Add(identity);
                    }
                }
            }, token);

            Project.IgnoredDependencies = _loadedAssemblies;

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            IsInitialized = true;
        }

        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the current <see cref="PackageService"/>.
        /// </summary>
        public async Task LoadPlugins(bool withAutoUpdate = false)
        {
            if (!Directory.Exists(_settings.PluginDirectory))
                return;

            var installedPlugins = await Project.GetInstalledPackagesAsync();
            foreach (var plugin in installedPlugins)
            {
                var updateStatus = PluginInstallStatus.NoAction;

                if (withAutoUpdate)
                {
                    try
                    {
                        updateStatus = await UpdatePlugin(plugin.PackageIdentity);
                    }
                    catch { }
                }

                if (updateStatus.IsLessThan(PluginInstallStatus.AppRestartRequired))
                    await LoadPlugin(plugin.PackageIdentity.Id);
            }
        }

        public async Task<PluginLoadResult> LoadPlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var result))
                return result;

            result = new(pluginId);

            // Get path to primary DLL
            var assemblyPath = Path.Combine(_settings.PluginDirectory, pluginId, $"{pluginId}.dll");
            if (string.IsNullOrWhiteSpace(assemblyPath))
                return result;

            try
            {
                // Load assembly and consider only public types that inherit from PackageHandlerBase
                AssemblyLoadContext localPluginLoadContext = new($"PluginLoadContext_{pluginId}", true);
                var assembly = localPluginLoadContext.LoadFromAssemblyPath(assemblyPath);

                _loadedPlugins.Add(pluginId, result);

                // Register all handlers
                await foreach (PackageHandlerBase handler in InstantiateAllPackageHandlers(assembly))
                {
                    _packageService.RegisterPackageHandler(handler);
                    result.PackageHandlers.Add(handler);
                }
            }
            catch (Exception ex)
            {
                _log.UnhandledException(ex, LogLevel.Error);
                return result;
            }

            return result;
        }

        /// <summary>
        /// Opens a stream for the provided plugin.
        /// </summary>
        /// <param name="pluginId">
        /// The package ID of the plugin to install.
        /// </param>
        /// <returns>
        /// A <see cref="DownloadResourceResult"/> containing a plugin stream and reader.
        /// </returns>
        public async Task<DownloadResourceResult> FetchPlugin(string pluginId, CancellationToken token = default)
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new Messages.PluginDownloadProgressMessage(
                    pluginId, 0, 1));

                var downloadedResource = await Project.DownloadPackageAsync(pluginId, token: token);

                WeakReferenceMessenger.Default.Send(new Messages.PluginDownloadProgressMessage(
                    pluginId, 1, 1));

                return downloadedResource;
            }
            catch (Exception ex)
            {
                _log.UnhandledException(ex, LogLevel.Error);
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginDownloadFailed));
            }

            return null;
        }

        /// <summary>
        /// Downloads and installs each plugin from the current NuGet feeds.
        /// </summary>
        /// <param name="pluginIds">
        /// The package IDs of the plugins to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force download or install, even if a plugin with the same ID
        /// is already installed.
        /// </param>
        public async Task InstallPlugins(IEnumerable<string> pluginIds, bool overwrite = false, CancellationToken token = default)
        {
            Directory.CreateDirectory(_settings.PluginDirectory);

            foreach (var pluginId in pluginIds)
            {
                token.ThrowIfCancellationRequested();

                await InstallPlugin(pluginId, overwrite, token);
            }
        }

        /// <summary>
        /// Downloads and installs a plugin from the current NuGet feed.
        /// </summary>
        /// <param name="pluginId">
        /// The package ID of the plugin to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force install, even if a plugin with the same ID
        /// and newer version is already installed.
        /// </param>
        /// <returns>
        /// Whether the plugin was installed successfully. <see langword="false"/> if the plugin
        /// was already installed and <paramref name="overwrite"/> was <see langword="false"/>.
        /// </returns>
        public async Task<PluginInstallStatus> InstallPlugin(string pluginId, bool overwrite = false, CancellationToken token = default)
        {
            try
            {
                var downloadedResource = await FetchPlugin(pluginId, token);

                WeakReferenceMessenger.Default.Send(new Messages.PluginInstallStartedMessage(pluginId));

                return await InstallPlugin(downloadedResource, overwrite, true, token);
            }
            catch (Exception ex)
            {
                _log.UnhandledException(ex, LogLevel.Error);
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginInstallFailed));
                return PluginInstallStatus.Failed;
            }
        }

        /// <summary>
        /// Installs the provided plugin.
        /// </summary>
        /// <param name="plugin">
        /// A <see cref="Stream"/> containing a NuGet package containing the plugin to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force install, even if a plugin with the same ID
        /// and newer version is already installed.
        /// </param>
        /// <returns>
        /// Whether the plugin was installed successfully. <see langword="false"/> if the plugin
        /// was already installed and <paramref name="overwrite"/> was <see langword="false"/>.
        /// </returns>
        public async Task<PluginInstallStatus> InstallPlugin(Stream plugin, bool overwrite = false, CancellationToken token = default)
        {
            try
            {
                // Read package metadata
                PackageArchiveReader reader = new(plugin);
                DownloadResourceResult downloadItem = new(plugin, reader, string.Empty);

                return await InstallPlugin(downloadItem, overwrite, true, token);
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, string.Empty, Messages.ErrorType.PluginInstallFailed));
                return PluginInstallStatus.Failed;
            }
        }

        /// <summary>
        /// Installs the provided plugin.
        /// </summary>
        /// <param name="downloadItem">
        /// A <see cref="DownloadResourceResult"/> of the plugin to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force install, even if a plugin with the same ID
        /// and newer version is already installed.
        /// </param>
        /// <param name="dispose">
        /// Whether to dispose of the provided <paramref name="downloadItem"/>.
        /// </param>
        /// <returns>
        /// Whether the plugin was installed successfully. <see langword="false"/> if the plugin
        /// was already installed and <paramref name="overwrite"/> was <see langword="false"/>.
        /// </returns>
        public async Task<PluginInstallStatus> InstallPlugin(DownloadResourceResult downloadItem, bool overwrite = false, bool dispose = true, CancellationToken token = default)
        {
            var pluginId = "[UNKNOWN]";
            PluginInstallStatus status;

            try
            {
                if (downloadItem?.Status is not (DownloadResourceResultStatus.AvailableWithoutStream or DownloadResourceResultStatus.Available))
                    throw new ArgumentException("The provided item failed to download", nameof(downloadItem));

                Directory.CreateDirectory(_settings.PluginDirectory);

                // Read package metadata
                var reader = downloadItem.PackageReader;
                var nuspec = reader.NuspecReader;

                pluginId = nuspec.GetId().ToString();
                var identity = nuspec.GetIdentity();

                WeakReferenceMessenger.Default.Send(new Messages.PluginInstallStartedMessage(pluginId));

                _projCtx.ActionType = overwrite ? NuGetActionType.Reinstall : NuGetActionType.Install;

                var installed = await Project.InstallPackageAsync(identity, downloadItem, _projCtx, token);
                status = Project.Entries[pluginId].InstallStatus;

                if (status == PluginInstallStatus.NoAction)
                {
                    WeakReferenceMessenger.Default.Send(
                        new Messages.WarningMessage($"A newer version of {pluginId} is already installed", pluginId));
                }
                else if (status == PluginInstallStatus.AppRestartRequired)
                {
                    WeakReferenceMessenger.Default.Send(new Messages.SuccessMessage(
                        $"{pluginId} will be installed the next time Fluent Store starts.",
                        pluginId, Messages.SuccessType.PluginInstallCompleted));
                }
                else
                {
                    // Fully installed, load the plugin now
                    await LoadPlugin(pluginId);

                    WeakReferenceMessenger.Default.Send(
                        Messages.SuccessMessage.CreateForPluginInstallCompleted(pluginId));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginInstallFailed));
                status = PluginInstallStatus.Failed;
            }
            finally
            {
                if (dispose)
                    downloadItem.Dispose();
            }

            return status;
        }

        public async Task<PluginInstallStatus> UninstallPlugin(string pluginId, CancellationToken token = default)
        {
            PluginInstallStatus status;

            try
            {
                status = await Project.UninstallPackageAsync(pluginId, _projCtx, token);

                if (status == PluginInstallStatus.NoAction)
                {
                    WeakReferenceMessenger.Default.Send(
                        new Messages.WarningMessage($"No action was taken because {pluginId} is not installed", pluginId));
                }
                else if (status == PluginInstallStatus.AppRestartRequired)
                {
                    WeakReferenceMessenger.Default.Send(new Messages.SuccessMessage(
                        $"{pluginId} will be uninstalled the next time Fluent Store starts.",
                        pluginId, Messages.SuccessType.PluginUninstallCompleted));
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(
                        Messages.SuccessMessage.CreateForPluginUninstallCompleted(pluginId));
                }
            }
            catch (Exception ex)
            {
                status = PluginInstallStatus.Failed;
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginUninstallFailed));
            }

            return status;
        }

        public async Task<PluginInstallStatus> UpdatePlugin(PackageIdentity pluginIdentity, CancellationToken token = default)
        {
            var findPackageResource = await FluentStoreRepo.GetResourceAsync<FindPackageByIdResource>(token);
            if (findPackageResource is null)
                return PluginInstallStatus.NoAction;

            var allVersions = await findPackageResource.GetAllVersionsAsync(pluginIdentity.Id, new(), global::NuGet.Common.NullLogger.Instance, token);
            var latestVersion = FluentStoreNuGetProject.SupportedSdkRange.FindBestMatch(allVersions);
            if (latestVersion is null || latestVersion <= pluginIdentity.Version)
                return PluginInstallStatus.NoAction;

            return await InstallPlugin(pluginIdentity.Id, true, token);
        }

        /// <summary>
        /// Handles any plugin operations that could not be completed during the last session.
        /// </summary>
        public async Task HandlePendingOperations()
        {
            if (!Directory.Exists(_settings.PluginDirectory))
                return;

            var pendingUninstalls = Project.Entries.Values
                .Where(e => e.UninstallStatus is PluginInstallStatus.AppRestartRequired or PluginInstallStatus.SystemRestartRequired)
                .ToList();
            foreach (var entry in pendingUninstalls)
            {
                await UninstallPlugin(entry.Id);
            }

            var pendingInstalls = Project.Entries.Values
                .Where(e => e.InstallStatus is PluginInstallStatus.AppRestartRequired or PluginInstallStatus.SystemRestartRequired)
                .ToList();
            foreach (var entry in pendingInstalls)
            {
                var pluginPath = Path.Combine(_settings.PluginDirectory, $"{entry.Id}.{entry.Version}.nupkg");
                using FileStream plugin = new(pluginPath, FileMode.Open);

                var installStatus = await InstallPlugin(plugin, true);
                if (installStatus.IsAtLeast(PluginInstallStatus.Completed))
                    File.Delete(pluginPath);
            }
        }

        public bool IsPluginInstalled(string pluginId) => _loadedPlugins.ContainsKey(pluginId);

        private Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            try
            {
                var currentAssembly = args.RequestingAssembly ?? Assembly.GetExecutingAssembly();
                string folderPath = Path.GetDirectoryName(currentAssembly.Location);
                string assemblyFile = new AssemblyName(args.Name).Name + ".dll";
                string assemblyPath = Path.Combine(folderPath, assemblyFile);

                if (File.Exists(assemblyPath))
                    assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, null);
            }

            return assembly;
        }

        private async IAsyncEnumerable<PackageHandlerBase> InstantiateAllPackageHandlers(Assembly pluginAssembly)
        {
            object[] ctorArgs = [_passwordVaultService];

            foreach (Type type in pluginAssembly.GetTypes()
                .Where(t => t.IsPublic && t.BaseType is not null && t.BaseType.IsAssignableTo(typeof(PackageHandlerBase))))
            {
                // Create a new instance of the handler
                var handler = (PackageHandlerBase)ActivatorUtilities.CreateInstance(Ioc.Default, type);
                if (handler is null)
                    continue;

                // Initialize handler
                if (handler is IAsyncInit needsInit)
                    await needsInit.InitAsync();

                // Register handler with the type name as its ID
                yield return handler;
            }
        }
    }

    public class PluginLoadResult
    {
        public PluginLoadResult(string id) : this(id, new()) { }

        public PluginLoadResult(string id, HashSet<PackageHandlerBase> packageHandlers)
        {
            Id = id;
            PackageHandlers = packageHandlers;
        }

        public string Id { get; }

        public HashSet<PackageHandlerBase> PackageHandlers { get; }
    }

    public enum PluginInstallStatus : byte
    {
        /// <summary>
        /// The plugin failed to install.
        /// </summary>
        Failed,

        /// <summary>
        /// The plugin was not installed because a newer version was already installed.
        /// </summary>
        NoAction,

        /// <summary>
        /// The plugin was installed but requires the app to restart.
        /// </summary>
        AppRestartRequired,

        /// <summary>
        /// The plugin was installed but requires the system to restart.
        /// </summary>
        SystemRestartRequired,

        /// <summary>
        /// The plugin has been completely installed and is ready to be loaded.
        /// </summary>
        Completed
    }
}

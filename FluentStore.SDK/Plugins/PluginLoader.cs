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

namespace FluentStore.SDK.Plugins
{
    public class PluginLoader : IAsyncInit
    {
        private static readonly NuGetFramework _targetFramework = NuGetFramework.Parse("net7.0-windows10.0.22621.0");

        private readonly HashSet<PackageIdentity> _loadedAssemblies =
        [
            new("NETStandard.Library", new NuGetVersion(2, 1, 0))
        ];

        private readonly ISettingsService _settings;
        private readonly PackageService _packageService;
        private readonly IPasswordVaultService _passwordVaultService;
        private readonly LoggerService _log;
        private readonly FluentStoreProjectContext _projCtx;
        private readonly Dictionary<string, PluginLoadResult> _loadedPlugins = [];

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
        public async Task LoadPlugins()
        {
            if (!Directory.Exists(_settings.PluginDirectory))
                return;

            var installedPlugins = await Project.GetInstalledPackagesAsync();
            foreach (var plugin in installedPlugins)
                await LoadPlugin(plugin.PackageIdentity.Id);
        }

        public async Task<PluginLoadResult> LoadPlugin(string pluginId)
        {
            if (_loadedPlugins.TryGetValue(pluginId, out var result))
                return result;

            result = new(pluginId);

            var pluginPackagePath = Path.Combine(_settings.PluginDirectory, pluginId, $"{pluginId}.nuspec");
            using Stream nuspecStream = new FileStream(pluginPackagePath, FileMode.Open);
            NuspecReader nuspec = new(nuspecStream);

            // Get path to primary DLL
            string assemblyPath = Path.Combine(Path.GetDirectoryName(pluginPackagePath), $"{nuspec.GetId()}.dll");
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
        /// Downloads and installs each plugin from the current NuGet feeds.
        /// </summary>
        /// <param name="pluginIds">
        /// Package IDs of the plugins to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force download or install, even if a plugin with the same ID
        /// is already installed.
        /// </param>
        public async Task InstallPlugins(IEnumerable<string> pluginIds, bool overwrite = false)
        {
            Directory.CreateDirectory(_settings.PluginDirectory);

            foreach (var pluginId in pluginIds)
            {
                await InstallPlugin(pluginId, overwrite);
            }
        }

        public async Task<bool> InstallPlugin(string pluginId, bool overwrite = false)
        {
            Directory.CreateDirectory(_settings.PluginDirectory);

            try
            {
                WeakReferenceMessenger.Default.Send(new Messages.PluginDownloadProgressMessage(
                    pluginId, 0, null));

                using var downloadedResource = await Project.DownloadPackageAsync(pluginId, VersionRange.All);
                await InstallPlugin(downloadedResource.PackageStream, overwrite);
            }
            catch (Exception ex)
            {
                _log.UnhandledException(ex, LogLevel.Error);
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginDownloadFailed));
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Installs the provided plugin.
        /// </summary>
        /// <param name="plugin">
        /// A <see cref="Stream"/> containing a zip archive of the plugin to install.
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
            string pluginId = "[UNKNOWN]";
            PluginInstallStatus status = PluginInstallStatus.Failed;
            Directory.CreateDirectory(_settings.PluginDirectory);

            try
            {
                // Read package metadata
                using PackageArchiveReader reader = new(plugin);
                NuspecReader nuspec = reader.NuspecReader;
                pluginId = nuspec.GetId().ToString();
                var identity = nuspec.GetIdentity();

                WeakReferenceMessenger.Default.Send(new Messages.PluginInstallStartedMessage(pluginId));

                _projCtx.ActionType = overwrite ? NuGetActionType.Reinstall : NuGetActionType.Install;

                var installed = await Project.InstallPackageAsync(identity,
                    new(plugin, reader, string.Empty), _projCtx, token);
                status = Project.Entries[pluginId].Status;

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

                return status;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                    ex, pluginId, Messages.ErrorType.PluginInstallFailed));
                return PluginInstallStatus.Failed;
            }
        }

        /// <summary>
        /// Installs any zipped plugins in the plugin directory.
        /// </summary>
        public async Task InstallPendingPlugins()
        {
            if (!Directory.Exists(_settings.PluginDirectory))
                return;

            var pendingPlugins = Project.Entries.Values
                .Where(e => e.Status is PluginInstallStatus.AppRestartRequired or PluginInstallStatus.SystemRestartRequired)
                .ToArray();

            foreach (var entry in pendingPlugins)
            {
                var pluginPath = Path.Combine(_settings.PluginDirectory, $"{entry.ToPackageIdentity()}.nupkg");
                using FileStream plugin = new(pluginPath, FileMode.Open);

                var installStatus = await InstallPlugin(plugin, true);
                if (installStatus.IsAtLeast(PluginInstallStatus.Completed))
                    File.Delete(pluginPath);
            }
        }

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

                // Enable or disable according to user settings
                handler.IsEnabled = _settings.GetPackageHandlerEnabledState(type.Name);

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

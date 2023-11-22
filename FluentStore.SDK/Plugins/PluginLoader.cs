using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging;
using OwlCore.ComponentModel;
using OwlCore.Storage.SystemIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins
{
    public class PluginLoader : IAsyncInit
    {
        private static readonly NuGetFramework _targetFramework = NuGetFramework.Parse("net7.0-windows10.0.22000.0");

        private readonly HashSet<string> _loadedAssemblies = new()
        {
            "NETStandard.Library",
        };

        private readonly ISettingsService _settings;
        private readonly PackageService _packageService;
        private readonly IPasswordVaultService _passwordVaultService;
        private readonly LoggerService _log;
        private readonly FluentStoreNuGetProject _proj;

        public bool IsInitialized { get; private set; }

        public PluginLoader(ISettingsService settings, PackageService packageService, IPasswordVaultService passwordVaultService, LoggerService log)
        {
            _settings = settings;
            _packageService = packageService;
            _passwordVaultService = passwordVaultService;
            _log = log;
            _proj = new(settings.PluginDirectory, _targetFramework);
        }

        public async Task InitAsync(CancellationToken token = default)
        {
            if (IsInitialized)
                return;

            await Task.Run(() =>
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var assemblyName = assembly.GetName().Name;
                    if (assemblyName is not null)
                        _loadedAssemblies.Add(assemblyName);
                }
            }, token);

            _proj.IgnoredDependencies = _loadedAssemblies;

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            IsInitialized = true;
        }

        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the provided <paramref name="packageHandlers"/>.
        /// </summary>
        /// <param name="packageHandlers">The dictionary to add loaded package handlers to.</param>
        public async Task<PluginLoadResult> LoadPlugins()
        {
            PluginLoadResult result = new();

            if (!Directory.Exists(_settings.PluginDirectory))
                return result;

            var installedPlugins = await _proj.GetInstalledPackagesAsync();
            foreach (var plugin in installedPlugins)
                LoadPlugin(plugin.PackageIdentity.Id, result);

            return result;
        }

        public PluginLoadResult LoadPlugin(string pluginId, PluginLoadResult? result = null)
        {
            result ??= new();

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
                var assembly = Assembly.LoadFile(assemblyPath);

                // Register all handlers
                foreach (PackageHandlerBase handler in InstantiateAllPackageHandlers(assembly))
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
        /// Downloads and installs each plugin from the list of URLS.
        /// </summary>
        /// <param name="_settings">
        /// The current application's settings.
        /// </param>
        /// <param name="pluginUrls">
        /// Download links to the plugins to install.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force download or install, even if a plugin with the same ID
        /// is already installed.
        /// </param>
        /// <param name="install">
        /// Whether to install the downloaded plugins. Only use this option at the
        /// start of app initialization, when no plugins are loaded yet.
        /// </param>
        public async Task InstallDefaultPlugins(IEnumerable<string> pluginUrls, bool install = true, bool overwrite = false)
        {
            SystemFolder pluginDirectory = new(Directory.CreateDirectory(_settings.PluginDirectory));

            Windows.Web.Http.HttpClient client = new();
            foreach (string url in pluginUrls)
            {
                try
                {
                    // This seems funky, but this is because Firebase encodes the slashes
                    // in the bucket path. This ensures we always get the last segment.
                    Flurl.Url pluginUrl = new(Flurl.Url.Decode(url, false));
                    string tempPluginId = Path.GetFileNameWithoutExtension(pluginUrl);

                    DataTransferProgress progress = new(prog =>
                    {
                        WeakReferenceMessenger.Default.Send(new Messages.PluginDownloadProgressMessage(
                            tempPluginId, (ulong)prog.BytesDownloaded, (ulong)prog.TotalBytes));
                    });

                    var remotePluginFile = AbstractStorageHelper.GetFileFromUrl(pluginUrl);
                    var pluginFile = await remotePluginFile.SaveLocally(pluginDirectory, progress, true);

                    if (install)
                        await InstallPlugin(await pluginFile.OpenStreamAsync(), overwrite);
                }
                catch (Exception ex)
                {
                    _log.UnhandledException(ex, LogLevel.Error);
                    WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                        ex, url, Messages.ErrorType.PluginDownloadFailed));
                }
            }
        }

        /// <summary>
        /// Installs the provided plugin.
        /// </summary>
        /// <param name="_settings">
        /// The current application's settings.
        /// </param>
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

                if (overwrite)
                    await _proj.UninstallPackageAsync(identity, null, token);

                var installed = await _proj.InstallPackageAsync(identity,
                    new(plugin, reader, "https://ipfs.askharoun.com"), null, token);

                status = _proj.Entries[pluginId].Status;
                if (status != PluginInstallStatus.AppRestartRequired)
                {
                    // App doesn't need to restart, we can finish the install now
                    status = PluginInstallStatus.Completed;

                    var result = LoadPlugin(pluginId);
                    _packageService.PackageHandlers.AddRange(result.PackageHandlers);
                }
                else
                {
                    // App will need to restart, make sure the package is
                    // in the plugin folder so the call to InstallPendingPlugins
                    // picks it up and finishes the install.
                    string pluginFileName = $"{pluginId}_{nuspec.GetVersion()}.nupkg";
                    using Stream stream = File.Open(Path.Combine(_settings.PluginDirectory, pluginFileName), FileMode.Create);
                    plugin.Position = 0;
                    plugin.CopyTo(stream);
                }

                WeakReferenceMessenger.Default.Send(
                    Messages.SuccessMessage.CreateForPluginInstallCompleted(pluginId));
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
        /// <param name="_settings">
        /// The current application's settings.
        /// </param>
        public async Task InstallPendingPlugins()
        {
            if (!Directory.Exists(_settings.PluginDirectory))
                return;

            var pendingPlugins = _proj.Entries.Values
                .Where(e => e.Status is PluginInstallStatus.AppRestartRequired or PluginInstallStatus.SystemRestartRequired);

            foreach (var entry in pendingPlugins)
            {
                var pluginPath = Path.Combine(_settings.PluginDirectory, entry.Id);
                using FileStream plugin = new(pluginPath, FileMode.Open);

                var installStatus = await InstallPlugin(plugin, true);
                if (installStatus.IsAtLeast(PluginInstallStatus.Completed))
                    File.Delete(pluginPath);
            }
        }

        private static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            var currentAssembly = args.RequestingAssembly ?? Assembly.GetExecutingAssembly();
            string folderPath = Path.GetDirectoryName(currentAssembly.Location);
            string assemblyFile = new AssemblyName(args.Name).Name + ".dll";
            string assemblyPath = Path.Combine(folderPath, assemblyFile);
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }

        private IEnumerable<PackageHandlerBase> InstantiateAllPackageHandlers(Assembly pluginAssembly)
        {
            object[] ctorArgs = new object[] { _passwordVaultService };

            foreach (Type type in pluginAssembly.GetTypes()
                .Where(t => t.BaseType.IsAssignableTo(typeof(PackageHandlerBase)) && t.IsPublic))
            {
                // Create a new instance of the handler
                var handler = (PackageHandlerBase)ActivatorUtilities.CreateInstance(Ioc.Default, type);
                if (handler == null)
                    continue;

                // Enable or disable according to user settings
                handler.IsEnabled = _settings.GetPackageHandlerEnabledState(type.Name);

                // Register handler with the type name as its ID
                yield return handler;
            }
        }
    }

    public class PluginLoadResult
    {
        public PluginLoadResult() : this(new()) { }

        public PluginLoadResult(HashSet<PackageHandlerBase> packageHandlers)
        {
            PackageHandlers = packageHandlers;
        }

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

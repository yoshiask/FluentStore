using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.ProjectManagement;
using NuGet.ProjectManagement.Projects;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using NuGet.Versioning;
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
    public class PluginLoader
    {
        private static readonly NuGetFramework _targetFramework = NuGetFramework.Parse("net7.0-windows10.0.22000.0");

        private readonly ISettingsService _settings;
        private readonly IPasswordVaultService _passwordVaultService;
        private readonly LoggerService _log;
        private readonly FluentStoreNuGetProject _proj;
        private readonly INuGetProjectContext _projContext;

        public PluginLoader(ISettingsService settings, IPasswordVaultService passwordVaultService, LoggerService log)
        {
            _settings = settings;
            _passwordVaultService = passwordVaultService;
            _log = log;
            _proj = new(settings.PluginDirectory, _targetFramework);
            _projContext = new FluentStoreProjectContext(_settings.PluginDirectory, _log);
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

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            var installedPlugins = await _proj.GetInstalledPackagesAsync(default);

            var pluginPackagePaths = installedPlugins.Select(p => Path.Combine(_settings.PluginDirectory, p.PackageIdentity.Id, $"{p.PackageIdentity.Id}.nuspec"));
            foreach (string pluginPackagePath in pluginPackagePaths)
            {
                using Stream nuspecStream = new FileStream(pluginPackagePath, FileMode.Open);
                NuspecReader nuspec = new(nuspecStream);

                // Get path to primary DLL.
                string assemblyPath = Path.Combine(Path.GetDirectoryName(pluginPackagePath), $"{nuspec.GetId()}.dll");
                if (string.IsNullOrWhiteSpace(assemblyPath))
                    continue;

                try
                {
                    // Load assembly and consider only types that inherit from PackageHandlerBase
                    // and are public.
                    var assembly = Assembly.LoadFile(assemblyPath);

                    foreach (PackageHandlerBase handler in InstantiateAllPackageHandlers(assembly))
                    {
                        // Register handler.
                        result.PackageHandlers.Add(handler);
                    }
                }
                catch (Exception ex)
                {
                    _log.UnhandledException(ex, LogLevel.Error);
                    continue;
                }
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

                WeakReferenceMessenger.Default.Send(new Messages.PluginInstallStartedMessage(pluginId));

                var installed = await _proj.InstallPackageAsync(nuspec.GetIdentity(),
                    new(plugin, reader, "https://ipfs.askharoun.com"), null, token);

                status = _proj.Entries[pluginId].Status;
                if (status != PluginInstallStatus.AppRestartRequired)
                {
                    // App won't need to restart, we can finish the install now
                    //archive.ExtractToDirectory(dir);
                    status = PluginInstallStatus.Completed;
                }
                else
                {
                    // App will need to restart, make sure the ZIP file is
                    // in the plugin folder so the call to InstallPendingPlugins
                    // picks it up and finishes the install.
                    string pluginFileName = $"{pluginId}_{nuspec.GetVersion()}.zip";
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

        /// <summary>
        /// Determines if the specified version is compatible, loosely following
        /// <see href="https://semver.org/">semantic versioning</see> rules.
        /// </summary>
        private static bool IsVersionCompatible(SemanticVersion curVer, SemanticVersion newVer)
        {
            // Doesn't make sense to compare null versions.
            if (curVer == null || newVer == null)
                return false;

            // Major version 0 is for development. These releases
            // need to have the same minor versions. Only the
            // patch version can be different.
            if (curVer.Major == 0 || newVer.Major == 0)
                return curVer.Major == newVer.Major
                    && curVer.Minor == newVer.Minor;

            // Otherwise, only the major version needs to match.
            return curVer.Major == newVer.Major;
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

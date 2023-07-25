using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Models;
using FluentStore.Services;
using Microsoft.Extensions.DependencyInjection;
using OwlCore.Storage.SystemIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public class PluginLoader
    {
        private static readonly Version _currentSdkVersion = typeof(PluginLoader).Assembly.GetName().Version;

        private readonly ISettingsService _settings;
        private readonly IPasswordVaultService _passwordVaultService;

        public PluginLoader(ISettingsService settings, IPasswordVaultService passwordVaultService)
        {
            _settings = settings;
            _passwordVaultService = passwordVaultService;
        }

        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the provided <paramref name="packageHandlers"/>.
        /// </summary>
        /// <param name="packageHandlers">The dictionary to add loaded package handlers to.</param>
        public PluginLoadResult LoadPlugins()
        {
            PluginLoadResult result = new();

            if (!Directory.Exists(_settings.PluginDirectory))
                return result;

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            foreach (string pluginMetadataPath in Directory.EnumerateFiles(_settings.PluginDirectory, PluginMetadata.MetadataFileName, SearchOption.AllDirectories))
            {
                // The plugin metadata specifies a relative path to the DLL containing
                // the actual plugin implementation.
                // This is to avoid unnecessarily loading and searching for package
                // handlers in DLLs that are only for dependencies (and also serves
                // to prevent loading the same plugin twice, if one plugin depends
                // on another).

                // Deserialize metadata.
                var metadata = PluginMetadata.DeserializeFromFile(pluginMetadataPath);

                // Ensure plugin is compatible with current Fluent Store SDK.
                if (!IsVersionCompatible(metadata.PluginVersion, _currentSdkVersion))
                    continue;

                // Get path to primary DLL.
                string assemblyPath = Path.Combine(Path.GetDirectoryName(pluginMetadataPath), metadata.PluginAssembly);
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
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(ex);
#endif
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
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(ex);
#endif
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
        public Task<PluginInstallStatus> InstallPlugin(Stream plugin, bool overwrite = false)
        {
            return Task.Run(delegate
            {
                string pluginId = "[UNKNOWN]";
                PluginInstallStatus status = PluginInstallStatus.Failed;

                try
                {
                    using ZipArchive archive = new(plugin);
                    var metadataEntry = archive.Entries.FirstOrDefault(
                        e => e.Name.Equals(PluginMetadata.MetadataFileName, StringComparison.OrdinalIgnoreCase));
                    if (metadataEntry == null)
                        return PluginInstallStatus.Failed;

                    // Read metadata before extracting archive.
                    PluginMetadata metadata;
                    using (var metadataEntryStream = metadataEntry.Open())
                    {
                        metadata = PluginMetadata.DeserializeFromStream(metadataEntryStream);
                        pluginId = metadata.Id;
                    }

                    WeakReferenceMessenger.Default.Send(new Messages.PluginInstallStartedMessage(pluginId));

                    Directory.CreateDirectory(_settings.PluginDirectory);

                    string dir = Path.Combine(_settings.PluginDirectory, pluginId);
                    if (Directory.Exists(dir))
                    {
                        string oldMetadataPath = Path.Combine(dir, PluginMetadata.MetadataFileName);
                        if (File.Exists(oldMetadataPath))
                        {
                            // Check if this version is newer than the currently installed one.
                            var oldMetadata = PluginMetadata.DeserializeFromFile(Path.Combine(dir, PluginMetadata.MetadataFileName));
                            overwrite |= metadata.PluginVersion > oldMetadata.PluginVersion;
                        }
                        else
                        {
                            // If it's missing metadata, it's an invalid install anyway.
                            overwrite = true;
                        }

                        if (overwrite)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch
                            {
                                status = PluginInstallStatus.AppRestartRequired;
                            }
                        }
                        else
                        {
                            return PluginInstallStatus.NoAction;
                        }
                    }

                    if (status != PluginInstallStatus.AppRestartRequired)
                    {
                        // App won't need to restart, we can finish the install now
                        archive.ExtractToDirectory(dir);
                        status = PluginInstallStatus.Completed;
                    }
                    else
                    {
                        // App will need to restart, make sure the ZIP file is
                        // in the plugin folder so the call to InstallPendingPlugins
                        // picks it up and finishes the install.
                        string pluginFileName = $"{metadata.Id}_{metadata.PluginVersion}.zip";
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
            });
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

            foreach (string pluginPath in Directory.GetFiles(_settings.PluginDirectory, "*.zip"))
            {
                using FileStream plugin = new(pluginPath, FileMode.Open);

                var installStatus = await InstallPlugin(plugin, true);
                if (installStatus.IsAtLeast(PluginInstallStatus.AppRestartRequired))
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
        private static bool IsVersionCompatible(Version curVer, Version newVer)
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

using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Models;
using FluentStore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace FluentStore.SDK
{
    public static class PluginLoader
    {
        private static readonly Type[] _ctorTypeList = new[] { typeof(IPasswordVaultService) };
        private static readonly Version _currentSdkVersion = typeof(PluginLoader).Assembly.GetName().Version;

        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the provided <paramref name="packageHandlers"/>.
        /// </summary>
        /// <param name="packageHandlers">The dictionary to add loaded package handlers to.</param>
        public static PluginLoadResult LoadPlugins(ISettingsService settings, IPasswordVaultService passwordVaultService)
        {
            PluginLoadResult result = new();

            if (!Directory.Exists(settings.PluginDirectory))
                return result;

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            foreach (string pluginMetadataPath in Directory.EnumerateFiles(settings.PluginDirectory, PluginMetadata.MetadataFileName, SearchOption.AllDirectories))
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
                if (!metadata.PluginVersion.IsVersionCompatible(_currentSdkVersion))
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

                    foreach (PackageHandlerBase handler in InstantiateAllPackageHandlers(assembly, settings, passwordVaultService))
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
        /// <param name="settings">
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
        public static async Task InstallDefaultPlugins(ISettingsService settings, IEnumerable<string> pluginUrls,
            bool install = true, bool overwrite = false)
        {
#if WINDOWS
            Windows.Web.Http.HttpClient
#else
            System.Net.Http.HttpClient
#endif
                client = new();
            foreach (string url in pluginUrls)
            {
                try
                {
                    // This seems funky, but this is because Firebase encodes the slashes
                    // in the bucket path. This ensures we always get the last segment.
                    Flurl.Url pluginUrl = new(Flurl.Url.Decode(url, false));
                    string tempPluginId = Path.GetFileNameWithoutExtension(pluginUrl.PathSegments.Last());

#if WINDOWS
                    var get = client.GetAsync(new(url), Windows.Web.Http.HttpCompletionOption.ResponseContentRead);
                    get.Progress = (op, prog) =>
                    {
                        WeakReferenceMessenger.Default.Send(new Messages.PluginDownloadProgressMessage(
                            tempPluginId, prog.BytesReceived, prog.TotalBytesToReceive));
                    };
                    var response = await get;

                    Directory.CreateDirectory(settings.PluginDirectory);
                    string pluginDownloadPath = Path.Combine(settings.PluginDirectory, tempPluginId) + ".zip";

                    FileStream pluginStream = new(pluginDownloadPath, FileMode.Create, FileAccess.ReadWrite);
                    using IRandomAccessStream outputStream = pluginStream.AsRandomAccessStream();
                    await response.Content.WriteToStreamAsync(outputStream);
                    await pluginStream.FlushAsync();
#else
                    var pluginStream = await client.GetStreamAsync(url);
#endif

                    if (install)
                        await InstallPlugin(settings, pluginStream, overwrite);
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
        /// <param name="settings">
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
        public static Task<bool> InstallPlugin(ISettingsService settings, Stream plugin, bool overwrite = false)
        {
            return Task.Run(delegate
            {
                string pluginId = "[UNKNOWN]";

                try
                {
                    using ZipArchive archive = new(plugin);
                    var metadataEntry = archive.Entries.FirstOrDefault(
                        e => e.Name.Equals(PluginMetadata.MetadataFileName, StringComparison.OrdinalIgnoreCase));
                    if (metadataEntry == null)
                        return false;

                    // Read metadata before extracting archive.
                    PluginMetadata metadata;
                    using (var metadataEntryStream = metadataEntry.Open())
                    {
                        metadata = PluginMetadata.DeserializeFromStream(metadataEntryStream);
                        pluginId = metadata.Id;
                    }

                    WeakReferenceMessenger.Default.Send(
                        new Messages.PluginInstallStartedMessage(pluginId));

                    Directory.CreateDirectory(settings.PluginDirectory);

                    string dir = Path.Combine(settings.PluginDirectory, pluginId);
                    if (Directory.Exists(dir))
                    {
                        // Check if this version is newer than the currently installed one.
                        var oldMetadata = PluginMetadata.DeserializeFromFile(Path.Combine(dir, PluginMetadata.MetadataFileName));
                        overwrite |= metadata.PluginVersion > oldMetadata.PluginVersion;

                        if (overwrite)
                            Directory.Delete(dir, true);
                        else
                            return false;
                    }

                    archive.ExtractToDirectory(dir);

                    WeakReferenceMessenger.Default.Send(
                        Messages.SuccessMessage.CreateForPluginInstallCompleted(pluginId));
                    return true;
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new Messages.ErrorMessage(
                        ex, pluginId, Messages.ErrorType.PluginInstallFailed));
                    return false;
                }
            });
        }

        /// <summary>
        /// Installs any zipped plugins in the plugin directory.
        /// </summary>
        /// <param name="settings">
        /// The current application's settings.
        /// </param>
        public static async Task InstallPendingPlugins(ISettingsService settings)
        {
            if (!Directory.Exists(settings.PluginDirectory))
                return;

            foreach (string pluginPath in Directory.GetFiles(settings.PluginDirectory, "*.zip"))
            {
                using FileStream plugin = new(pluginPath, FileMode.Open);

                bool installed = await InstallPlugin(settings, plugin);
                if (installed)
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

        private static IEnumerable<PackageHandlerBase> InstantiateAllPackageHandlers(Assembly pluginAssembly, ISettingsService settings, IPasswordVaultService passwordVaultService)
        {
            object[] ctorArgs = new object[] { passwordVaultService };

            foreach (Type type in pluginAssembly.GetTypes()
                .Where(t => t.BaseType.IsAssignableTo(typeof(PackageHandlerBase)) && t.IsPublic))
            {
                var ctr = type.GetConstructor(_ctorTypeList);
                if (ctr == null)
                    continue;

                // Create a new instance of the handler
                var handler = (PackageHandlerBase)ctr.Invoke(ctorArgs);
                if (handler == null)
                    continue;

                // Enable or disable according to user settings
                handler.IsEnabled = settings.GetPackageHandlerEnabledState(type.Name);

                // Register handler with the type name as its ID
                yield return handler;
            }
        }

        /// <summary>
        /// Determines if the specified version is compatible, loosely following
        /// <see href="https://semver.org/">semantic versioning</see> rules.
        /// </summary>
        private static bool IsVersionCompatible(this Version curVer, Version newVer)
        {
            // Doesn't make sense to compare null versions.
            if (curVer == null || newVer == null)
                return false;

            // Major version 0 is for development. These releases
            // need to have the same major, minor, and build
            // versions. Only the patch version can be different.
            if (curVer.Major == 0 || newVer.Major == 0)
                return curVer.Major == newVer.Major
                    && curVer.Minor == newVer.Minor
                    && curVer.Build == newVer.Build;

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
}

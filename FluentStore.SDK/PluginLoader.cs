using FluentStore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public static class PluginLoader
    {
        private static readonly Type[] _ctorTypeList = new[] { typeof(IPasswordVaultService) };

        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the provided <paramref name="packageHandlers"/>.
        /// </summary>
        /// <param name="packageHandlers">The dictionary to add loaded package handlers to.</param>
        public static PluginLoadResult LoadPlugins(ISettingsService settings, IPasswordVaultService passwordVaultService)
        {
            PluginLoadResult result = new();

            // Make sure that the runtime looks for plugin dependencies
            // in the plugin's folder.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += LoadFromSameFolder;

            foreach (string pluginInfoPath in Directory.EnumerateFiles(settings.PluginDirectory, "*.txt", SearchOption.AllDirectories))
            {
                // The plugin info file is a plain text file where each line is
                // a relative path to the DLLs containing the actual implementation.
                // This is to avoid unnecessarily loading and searching for package
                // handlers in DLLs that are only for dependencies (and also serves
                // to prevent loading the same plugin twice, if one plugin depends
                // on another.
                string pluginPath = Path.GetDirectoryName(pluginInfoPath);
                string[] dllRelativePaths = File.ReadAllLines(pluginInfoPath);
                foreach (string dllRelativePath in dllRelativePaths)
                {
                    string assemblyPath = Path.Combine(pluginPath, dllRelativePath);
                    if (string.IsNullOrWhiteSpace(assemblyPath))
                        continue;

                    try
                    {
                        // Load assembly and consider only types that inherit from PackageHandlerBase
                        // and are public.
                        var assembly = Assembly.LoadFile(assemblyPath);

                        foreach (PackageHandlerBase handler in InstantiateAllPackageHandlers(assembly, settings, passwordVaultService))
                        {
                            // Register handler
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
            }

            return result;
        }

        public static async Task InstallDefaultPlugins(ISettingsService settings, IEnumerable<string> pluginUrls)
        {
            System.Net.Http.HttpClient client = new();
            foreach (string url in pluginUrls)
            {
                try
                {
                    var response = await client.GetStreamAsync(url);
                    await InstallPlugin(settings, response, Path.GetFileNameWithoutExtension(url));
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(ex);
#endif
                }
            }
        }

        /// <summary>
        /// Installs the provided plugin using the plugin ID.
        /// </summary>
        /// <param name="settings">
        /// The current application's settings.
        /// </param>
        /// <param name="plugin">
        /// The plugin the install.
        /// </param>
        /// <param name="pluginId">
        /// The ID of the plugin, used to determine if it's already installed.
        /// </param>
        /// <param name="overwrite">
        /// Whether to force install, even if a plugin with the same ID
        /// is already installed.
        /// </param>
        /// <returns>
        /// Whether the plugin was installed. <see langword="false"/> if the plugin
        /// was already installed.
        /// </returns>
        public static Task<bool> InstallPlugin(ISettingsService settings, Stream plugin, string pluginId, bool overwrite = false)
        {
            return Task.Run(delegate
            {
                string dir = Path.Combine(settings.PluginDirectory, pluginId);
                if (Directory.Exists(dir))
                {
                    if (overwrite)
                        Directory.Delete(dir, true);
                    else
                        return false;
                }

                using ZipArchive archive = new(plugin);
                archive.ExtractToDirectory(dir);
                return true;
            });
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

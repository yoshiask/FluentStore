using FluentStore.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FluentStore.SDK
{
    public static class PluginLoader
    {
        /// <summary>
        /// Attempts to load all plugins located in <see cref="ISettingsService.PluginDirectory"/>
        /// and registers all loaded package handlers with the provided <paramref name="packageHandlers"/>.
        /// </summary>
        /// <param name="packageHandlers">The dictionary to add loaded package handlers to.</param>
        public static Dictionary<string, PackageHandlerBase> LoadPlugins(ISettingsService settings)
        {
            var emptyTypeList = Array.Empty<Type>();
            var emptyObjectList = Array.Empty<object>();
            Dictionary<string, PackageHandlerBase> packageHandlers = new();

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
                        foreach (Type type in assembly.GetTypes()
                            .Where(t => t.BaseType.IsAssignableTo(typeof(PackageHandlerBase)) && t.IsPublic))
                        {
                            var ctr = type.GetConstructor(emptyTypeList);
                            if (ctr == null)
                                continue;

                            // Create a new instance of the handler
                            var handler = (PackageHandlerBase)ctr.Invoke(emptyObjectList);
                            if (handler == null)
                                continue;

                            // Enable or disable according to user settings
                            handler.IsEnabled = settings.GetPackageHandlerEnabledState(type.Name);

                            // Register handler with the type name as its ID
                            packageHandlers.Add(type.Name, handler);
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

            currentDomain.AssemblyResolve -= LoadFromSameFolder;
            return packageHandlers;
        }

        private static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(args.RequestingAssembly.Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}

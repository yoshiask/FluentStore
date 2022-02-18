using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace FluentStore.SDK
{
    public static class PluginLoader
    {

        public static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(args.RequestingAssembly.Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
    }
}

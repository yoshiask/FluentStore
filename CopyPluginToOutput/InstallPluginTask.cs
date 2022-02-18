using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CopyPluginToOutput
{
    public class InstallPluginTask : Microsoft.Build.Utilities.Task
    {
        [Required] public string OutputDir { get; set; }
        [Required] public string PluginDll { get; set; }

        public string PluginAssemblyName { get; set; }

        public override bool Execute()
        {
            string pluginAssemblyName = PluginAssemblyName ?? Path.GetFileNameWithoutExtension(PluginDll);
            string pluginDll = Path.Combine(OutputDir, Path.ChangeExtension(PluginDll, ".dll"));

            if (!Directory.Exists(OutputDir))
            {
                Log.LogError($"The directory '{OutputDir}' could not be found.");
                return false;
            }
            else if (!File.Exists(pluginDll))
            {
                Log.LogError($"The file '{pluginDll}' could not be found.");
                return false;
            }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(localAppData, "FluentStoreBeta", "Plugins", pluginAssemblyName));
            if (dir.Exists)
                dir.Delete(true);
            dir.Create();

            // Copy output directory to plugin directory
            CopyAll(OutputDir, dir.FullName);

            // Write plugin info file
            File.WriteAllText(Path.Combine(dir.FullName, pluginAssemblyName + ".txt"), PluginDll);

            Log.LogMessage($"Installed plugin '{PluginAssemblyName}' to '{dir.FullName}'");
            
            return true;
        }

        private static void CopyAll(string SourcePath, string DestinationPath)
        {
            string sep = Path.DirectorySeparatorChar.ToString();
            if (!SourcePath.EndsWith(sep)) SourcePath += sep;
            if (!DestinationPath.EndsWith(sep)) DestinationPath += sep;

            string[] directories = Directory.GetDirectories(SourcePath, "*.*", SearchOption.AllDirectories);

            Parallel.ForEach(directories, dirPath =>
            {
                Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));
            });

            string[] files = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);

            Parallel.ForEach(files, newPath =>
            {
                if (Path.GetFileNameWithoutExtension(newPath) != "FluentStore.SDK")
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath));
            });
        }
    }
}

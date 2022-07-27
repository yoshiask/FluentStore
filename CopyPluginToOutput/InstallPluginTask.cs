using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CopyPluginToOutput
{
    public class InstallPluginTask : Microsoft.Build.Utilities.Task
    {
        [Required] public string OutputDir { get; set; }
        [Required] public string PluginAssemblyName { get; set; }

        public override bool Execute()
        {
            if (!Directory.Exists(OutputDir))
            {
                Log.LogError($"The directory '{OutputDir}' could not be found.");
                return false;
            }

            var mainDrive = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(mainDrive.FullName, "ProgramData", "FluentStoreBeta",
                "Plugins", PluginAssemblyName));
            if (dir.Exists)
                dir.Delete(true);
            dir.Create();

            // Copy output directory to plugin directory
            CopyAll(OutputDir, dir.FullName);

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

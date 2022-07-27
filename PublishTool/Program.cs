using FluentStore.SDK.Models;
using System.IO.Compression;

// List architectures
var archs = new[] { "x64", "x86", "arm64" };

// Get folder containing plugin projects
string curDir = Environment.CurrentDirectory;
string targetFramework = Path.GetFileName(curDir);
string sourcesDir = Path.GetFullPath(Path.Combine(curDir, @"..\..\..\..\..\Sources"));
string zipOutDir = Path.Combine(sourcesDir, "output");

Directory.CreateDirectory(zipOutDir);

Console.WriteLine($"Searching for plugins in '{sourcesDir}'");

Parallel.ForEach(Directory.GetDirectories(sourcesDir).Where(d => d != zipOutDir), pluginSrcDir =>
{
    // Get build output directories
    var metadata = PluginMetadata.DeserializeFromFile(Path.Combine(pluginSrcDir, PluginMetadata.MetadataFileName));

    Console.WriteLine($"Read metadata for {metadata.DisplayName} ({metadata.Id}, {metadata.PluginVersion})");

    foreach (var arch in archs)
    {
        string pluginTarget = $"{metadata.Id}_{metadata.PluginVersion}_{arch}";

        string pluginOutDir = Path.Combine(pluginSrcDir, "bin", arch, "Debug", targetFramework);
        string zipPath = Path.Combine(zipOutDir, pluginTarget + ".zip");

        Console.WriteLine($"[{pluginTarget}] Creating archive...");
        ZipFile.CreateFromDirectory(pluginOutDir, zipPath);
        Console.WriteLine($"[{pluginTarget}] Saved to '{zipPath}'");
    }
});

var foreColor = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine($"Finished archiving plugins to '{zipOutDir}'");

Console.ForegroundColor = foreColor;

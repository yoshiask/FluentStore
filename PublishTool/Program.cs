using CliWrap;
using FluentStore.SDK.Models;
using Spectre.Console;
using System.IO.Compression;

// List architectures
string[] archs = new[] { "x64", "x86", "arm64" };

// Get folder containing plugin projects
string curDir = Environment.CurrentDirectory;
string targetFramework = Path.GetFileName(curDir);
string sourcesDir = Path.GetFullPath(Path.Combine(curDir, @"..\..\..\..\..\Sources"));
string zipOutDir = Path.Combine(sourcesDir, "output");

Directory.CreateDirectory(zipOutDir);

Console.WriteLine($"Searching for plugins in '{sourcesDir}'");

await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var pluginDirs = Directory.GetDirectories(sourcesDir).Where(d => d != zipOutDir);
        await Parallel.ForEachAsync(pluginDirs, async (pluginSrcDir, token) =>
        {
            // Get build output directories
            var metadata = PluginMetadata.DeserializeFromFile(Path.Combine(pluginSrcDir, PluginMetadata.MetadataFileName));

            var pluginProgress = ctx.AddTask($"{metadata.DisplayName} ({metadata.Id}, {metadata.PluginVersion})");
            pluginProgress.MaxValue = archs.Length * 2;
            pluginProgress.StartTask();

            foreach (var arch in archs)
            {
                var rid = $"win-{arch}";
                var pluginTarget = $"{metadata.Id}_{metadata.PluginVersion}_{arch}";
                var pluginOutDir = Path.Combine(pluginSrcDir, "bin", "Debug", targetFramework, rid);
                var zipPath = Path.Combine(zipOutDir, pluginTarget + ".zip");

                if (File.Exists(zipPath))
                {
                    pluginProgress.Increment(2);
                    continue;
                }

                var buildResult = await Cli.Wrap("dotnet")
                    .WithArguments(new[] { "build", "-r", rid, "--no-self-contained" })
                    .WithWorkingDirectory(pluginSrcDir)
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync(token);
                if (buildResult.ExitCode == 0)
                    pluginProgress.Increment(1);
                else
                    continue;

                File.Delete(zipPath);
                ZipFile.CreateFromDirectory(pluginOutDir, zipPath);
                
                pluginProgress.Increment(1);
            }

            pluginProgress.StopTask();
        }).ConfigureAwait(false);
    });

WriteSuccess($"Finished packaging plugins to '{zipOutDir}'");

static void WriteSuccess(string message)
{
    var foreColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(message);
    Console.ForegroundColor = foreColor;
}

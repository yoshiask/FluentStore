using CliWrap;
using FluentStore.SDK.Models;
using Spectre.Console;

// List architectures
string[] archs = new[] { "x64", "x86", "arm64" };

var argParser = Meziantou.Framework.CommandLineParser.Current;
bool verbose = argParser.HasArgument("v");
bool saveLogs = argParser.HasArgument("-log");
bool force = argParser.HasArgument("f") || argParser.HasArgument("-force");

// Get folder containing plugin projects
string curDir = Environment.CurrentDirectory;
string targetFramework = Path.GetFileName(curDir);
string sourcesDir = Path.GetFullPath(Path.Combine(curDir, @"..\..\..\..\..\Sources"));
string pluginOutDir = Path.Combine(sourcesDir, "output");

Directory.CreateDirectory(pluginOutDir);

List<string> errors = new();

await AnsiConsole.Status()
    .StartAsync($"Searching for plugins in '{sourcesDir}'...", async (ctx) =>
    {
        var pluginDirs = Directory.GetDirectories(sourcesDir).Where(d => d != pluginOutDir);
        
        foreach (var pluginSrcDir in pluginDirs)
        {
            ctx.Status($"Preparing plugin project...");

            // Get build output directories
            var metadata = PluginMetadata.DeserializeFromFile(Path.Combine(pluginSrcDir, PluginMetadata.MetadataFileName));

            Rule header = new($"{metadata.DisplayName} [grey]({metadata.Id}, {metadata.PluginVersion})[/]")
            {
                Justification = Justify.Left,
                Border = BoxBorder.Ascii,
            };
            AnsiConsole.Write(header);

            var fileName = $"{metadata.Id}_{metadata.PluginVersion}";
            var nupkgSrcPath = Path.Combine(pluginSrcDir, "bin", "Debug", $"{metadata.Id}.{metadata.PluginVersion.Major}.{metadata.PluginVersion.Minor}.{metadata.PluginVersion.Build}.nupkg");
            var nupkgOutPath = Path.Combine(pluginOutDir, $"{fileName}.nupkg");


            if (!force && File.Exists(nupkgOutPath))
            {
                AnsiConsole.MarkupLine("Skipping: Plugin package found in cache");
                AnsiConsole.WriteLine();
                continue;
            }

            ctx.Status($"Packing {header.Title}...");

            PipeTarget buildLogOutPipe = PipeTarget.Null;
            PipeTarget buildLogErrPipe = PipeTarget.Create(async (s, t) =>
            {
                StreamReader reader = new(s);
                var text = await reader.ReadToEndAsync(t);
                AnsiConsole.Markup("[red]" + text + "[/]");
            });

            if (verbose)
            {
                buildLogOutPipe = PipeTarget.Create(async (s, t) =>
                {
                    StreamReader reader = new(s);
                    var text = await reader.ReadToEndAsync(t);
                    AnsiConsole.Write(text);
                });
            }
            if (saveLogs)
            {
                var buildLogOutPath = Path.Combine(pluginOutDir, $"{fileName}_out.txt");
                var buildLogErrPath = Path.Combine(pluginOutDir, $"{fileName}_err.txt");

                buildLogOutPipe = PipeTarget.Merge(buildLogOutPipe, PipeTarget.ToFile(buildLogOutPath));
                buildLogErrPipe = PipeTarget.Merge(buildLogErrPipe, PipeTarget.ToFile(buildLogErrPath));
            }

            var buildResult = await Cli.Wrap("dotnet")
                .WithArguments(new[] { "pack", "-c", "Debug" })
                .WithWorkingDirectory(pluginSrcDir)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(buildLogOutPipe)
                .WithStandardErrorPipe(buildLogErrPipe)
                .ExecuteAsync();
            if (buildResult.ExitCode != 0)
            {
                AnsiConsole.MarkupLine($"[red]Failed to pack {metadata.Id} with exit code 0x{buildResult.ExitCode:X8}[/]");
                continue;
            }

            File.Delete(nupkgOutPath);
            File.Copy(nupkgSrcPath, nupkgOutPath);

            AnsiConsole.MarkupLine($"[green]Successfully packed {metadata.Id} to '[link]{nupkgOutPath}[/]'[/]");
            AnsiConsole.WriteLine();
        }
    });

AnsiConsole.MarkupLine($"[green]Finished packaging plugins to '[link]{pluginOutDir}[/]'[/]");

foreach (var error in errors)
{
    AnsiConsole.WriteException(new Exception(error));
}

static void WriteSuccess(string message)
{
    var foreColor = Console.ForegroundColor;
    AnsiConsole.Foreground = ConsoleColor.Green;
    AnsiConsole.MarkupLine(message);
    AnsiConsole.Foreground = foreColor;
}

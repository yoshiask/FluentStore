using CliWrap;
using FluentStore.SDK.Plugins;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.Frameworks;
using NuGet.Versioning;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using Spectre.Console;

// List architectures
string[] archs = ["x64", "x86", "arm64"];

var argParser = Meziantou.Framework.CommandLineParser.Current;
var pluginId = argParser.GetArgument("-id");
var repoPath = argParser.GetArgument("-repo");
bool verbose = argParser.HasArgument("v");
bool saveLogs = argParser.HasArgument("-log");
bool install = argParser.HasArgument("-install");
bool force = argParser.HasArgument("f") || argParser.HasArgument("-force");

// Get folder containing plugin projects
string curDir = Environment.CurrentDirectory;
string sourcesDir = Path.GetFullPath(Path.Combine(repoPath ?? curDir, "Sources"));
string pluginOutDir = Path.Combine(sourcesDir, "output");

MSBuildLocator.RegisterDefaults();

Directory.CreateDirectory(pluginOutDir);

List<string> errors = [];

AnsiConsole.MarkupLine($"Searching for plugins in '[link]{sourcesDir}[/]'...");

await AnsiConsole.Status()
    .StartAsync("Starting...", async (ctx) =>
    {
        IEnumerable<string> pluginCsprojPaths = pluginId is not null
            ? [Path.Combine(sourcesDir, pluginId, $"{pluginId}.csproj")]
            : Directory.EnumerateFiles(sourcesDir, "*.csproj", SearchOption.AllDirectories);

        foreach (var pluginCsprojPath in pluginCsprojPaths)
            await BuildPlugin(ctx, pluginCsprojPath);
    });

AnsiConsole.MarkupLine($"[green]Finished packaging plugins to '[link]{pluginOutDir}[/]'[/]");

foreach (var error in errors)
{
    AnsiConsole.WriteException(new Exception(error));
}

async Task BuildPlugin(StatusContext ctx, string pluginCsprojPath)
{
    ctx.Status($"Preparing plugin project...");

    // Open csproj with MSBuild
    var pluginSrcDir = Path.GetDirectoryName(pluginCsprojPath)!;
    Project csproj = new(pluginCsprojPath);

    // Get relevant project properties
    var id = csproj.GetPropertyValue("PackageId");
    var title = csproj.GetPropertyValue("Title");
    var version = csproj.GetPropertyValue("PackageVersion");

    var identity = $"{id}.{version}";

    Rule header = new($"{title} [grey]({id}, {version})[/]")
    {
        Justification = Justify.Left,
        Border = BoxBorder.Ascii,
    };
    AnsiConsole.Write(header);

    var fileName = $"{identity}.nupkg";
    var nupkgFilePath = Path.Combine(pluginOutDir, fileName);
    if (!force && File.Exists(nupkgFilePath))
    {
        AnsiConsole.MarkupLine("Skipping: Plugin package found in cache");
        AnsiConsole.WriteLine();
        return;
    }

    ctx.Status($"Packing {header.Title}...");

    PipeTarget buildLogOutPipe = PipeTarget.Null;
    PipeTarget buildLogErrPipe = PipeTarget.Create(async (s, t) =>
    {
        StreamReader reader = new(s);
        var text = await reader.ReadToEndAsync(t);
        AnsiConsole.MarkupInterpolated($"[red]{text}[/]");
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
        .WithArguments(["pack", "-c", "Debug", "-o", pluginOutDir])
        .WithWorkingDirectory(pluginSrcDir)
        .WithValidation(CommandResultValidation.None)
        .WithStandardOutputPipe(buildLogOutPipe)
        .WithStandardErrorPipe(buildLogErrPipe)
        .ExecuteAsync();
    if (buildResult.ExitCode != 0)
    {
        AnsiConsole.MarkupLine($"[red]Failed to pack {id} with exit code 0x{buildResult.ExitCode:X8}[/]");
        return;
    }

    AnsiConsole.MarkupLine($"[green]Successfully packed {id}[/]");

    if (install)
    {
        var mainDrive = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        var appDataDir = Path.Combine(mainDrive?.FullName ?? "C:", "ProgramData", "FluentStoreBeta");
        SystemFolder pluginDir = new(Path.Combine(appDataDir, "Plugins"));

        var statusFile = await pluginDir.GetFirstByNameAsync("status.tsv") as IFile;
        var entries = await PluginStatusRecord.ReadAsync(statusFile);

        SystemFolder outputDir = new(pluginOutDir);
        var pluginFile = await outputDir.GetFirstByNameAsync(fileName) as IFile;
        await pluginDir.CreateCopyOfAsync(pluginFile!, true);
        
        entries[id] = new(id, NuGetVersion.Parse(version), NuGetFramework.UnsupportedFramework,
            PluginInstallStatus.AppRestartRequired, PluginInstallStatus.NoAction, VersionRange.All);

        await entries.WriteAsync(statusFile);

        AnsiConsole.MarkupLine($"[green]Successfully installed {id}[/]");
    }

    AnsiConsole.WriteLine();
}

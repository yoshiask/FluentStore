using CliWrap;
using FluentStore.SDK.Plugins;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.Frameworks;
using NuGet.Versioning;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using Spectre.Console;

namespace PublishTool.Commands;

public class Plugin
{
    private readonly IAnsiConsole _console;
    private readonly string _sourcesDir;
    private readonly string _pluginOutDir;
    private readonly string? _pluginId;
    private readonly string _repoPath;
    private readonly bool _verbose;
    private readonly bool _saveLogs;
    private readonly bool _install;
    private readonly bool _force;

    public Plugin(CommandLineParser argParser, IAnsiConsole console) : this(
        console,
        pluginId: argParser.GetArgument("-id"),
        repoPath: argParser.GetArgument("-repo") ?? Environment.CurrentDirectory,
        verbose: argParser.HasArgument("v"),
        saveLogs: argParser.HasArgument("-log"),
        install: argParser.HasArgument("-install"),
        force: argParser.HasArgument("f") || argParser.HasArgument("-force"))
    {
    }

    public Plugin(IAnsiConsole console, string? pluginId, string repoPath, bool verbose, bool saveLogs, bool install, bool force)
    {
        _console = console;

        // Get folder containing plugin projects
        _pluginId = pluginId;
        _verbose = verbose;
        _saveLogs = saveLogs;
        _install = install;
        _force = force;

        _repoPath = repoPath;
        _sourcesDir = Path.GetFullPath(Path.Combine(repoPath, "Sources"));
        _pluginOutDir = Path.Combine(_sourcesDir, "output");
    }

    public async Task BuildPluginsAsync()
    {
        MSBuildLocator.RegisterDefaults();

        Directory.CreateDirectory(_pluginOutDir);

        List<string> errors = [];

        _console.MarkupLine($"Searching for plugins in '[link]{_sourcesDir}[/]'...");

        await _console.Status()
            .StartAsync("Starting...", async (ctx) =>
            {
                IEnumerable<string> pluginCsprojPaths = _pluginId is not null
                    ? [Path.Combine(_sourcesDir, _pluginId, $"{_pluginId}.csproj")]
                    : Directory.EnumerateFiles(_sourcesDir, "*.csproj", SearchOption.AllDirectories);

                foreach (var pluginCsprojPath in pluginCsprojPaths)
                    await BuildPluginAsync(ctx, _pluginOutDir, pluginCsprojPath, _force, _verbose, _saveLogs, _install);
            });

        _console.MarkupLine($"[green]Finished packaging plugins to '[link]{_pluginOutDir}[/]'[/]");

        foreach (var error in errors)
        {
            _console.WriteException(new Exception(error));
        }
    }

    public async Task BuildPluginAsync(StatusContext ctx, string pluginOutDir, string pluginCsprojPath, bool force, bool verbose, bool saveLogs, bool install)
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
        _console.Write(header);

        var fileName = $"{identity}.nupkg";
        var nupkgFilePath = Path.Combine(pluginOutDir, fileName);
        if (!force && File.Exists(nupkgFilePath))
        {
            _console.MarkupLine("Skipping: Plugin package found in cache");
            _console.WriteLine();
            return;
        }

        ctx.Status($"Packing {header.Title}...");

        PipeTarget buildLogOutPipe = PipeTarget.Null;
        PipeTarget buildLogErrPipe = PipeTarget.Create(async (s, t) =>
        {
            StreamReader reader = new(s);
            var text = await reader.ReadToEndAsync(t);
            _console.MarkupInterpolated($"[red]{text}[/]");
        });

        if (verbose)
        {
            buildLogOutPipe = PipeTarget.Create(async (s, t) =>
            {
                StreamReader reader = new(s);
                var text = await reader.ReadToEndAsync(t);
                _console.Write(text);
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
            _console.MarkupLine($"[red]Failed to pack {id} with exit code 0x{buildResult.ExitCode:X8}[/]");
            return;
        }

        _console.MarkupLine($"[green]Successfully packed {id}[/]");

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

            _console.MarkupLine($"[green]Successfully installed {id}[/]");
        }

        _console.WriteLine();
    }
}

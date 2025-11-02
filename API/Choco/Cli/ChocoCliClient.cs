using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chocolatey.Models;
using CliWrap;
using NuGet.Versioning;

namespace Chocolatey.Cli;

public partial class ChocoCliClient : IChocoPackageService
{
    private static readonly Regex _rxProgress = new(@"Progress: Downloading (?<id>[\w.\-_]+) (?<ver>\d+(\.\d+){0,3}(-[\w\d]+)?)\.\.\. (?<prog>\d{1,3})%", RegexOptions.Compiled);

    public bool NoOp { get; set; } = false;
    
    public async Task<bool> InstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null)
    {
        List<string> args = ["install", id];

        ChocoArgumentsBuilder argBuilder = new()
        {
            Version = version,
            Yes = true,
            LimitOutput = true,
            NoOp = NoOp
        };

        argBuilder.Build(args);
        
        var result = await CliWrap.Cli.Wrap(ChocoArgumentsBuilder.CHOCO_EXE)
            .WithArguments(args)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(HandleStdOut))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
        
        return result.ExitCode is 0;

        void HandleStdOut(string line)
        {
            if (progress is null)
                return;

            var match = _rxProgress.Match(line);
            if (!match.Success)
                return;

            var id = match.Groups["id"].Value;
            var version = NuGetVersion.Parse(match.Groups["ver"].Value);
            var percentage = int.Parse(match.Groups["prog"].Value);
            progress.Report(new(id, version, percentage));
        }
    }

    public async Task<bool> UninstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null)
    {
        List<string> args = ["uninstall", id];

        ChocoArgumentsBuilder argBuilder = new()
        {
            Version = version,
            Yes = true,
            LimitOutput = true,
            NoOp = NoOp
        };

        argBuilder.Build(args);

        var result = await CliWrap.Cli.Wrap(ChocoArgumentsBuilder.CHOCO_EXE)
            .WithArguments(args)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(HandleStdOut))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        return result.ExitCode is 0;

        void HandleStdOut(string line)
        {
            if (progress is null)
                return;

            var match = _rxProgress.Match(line);
            if (!match.Success)
                return;

            var id = match.Groups["id"].Value;
            var version = NuGetVersion.Parse(match.Groups["ver"].Value);
            var percentage = int.Parse(match.Groups["prog"].Value);
            progress.Report(new(id, version, percentage));
        }
    }

    public async Task<bool> UpgradeAllAsync(IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpgradeAsync(string id, IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<(string Id, NuGetVersion Version)> ListAsync()
    {
        ChocoArgumentsBuilder argBuilder = new()
        {
            LimitOutput = true,
        };

        argBuilder.Build(["list"]);

        List<(string, NuGetVersion)> installedPackages = [];

        var result = await CliWrap.Cli.Wrap(ChocoArgumentsBuilder.CHOCO_EXE)
            .WithArguments((List<string>)["list"])
            .WithStandardOutputPipe(PipeTarget.ToDelegate(HandleStdOut))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        foreach (var p in installedPackages)
            yield return p;

        void HandleStdOut(string line)
        {
            int separatorIdx = line.IndexOf('|');
            var id = line.Substring(0, separatorIdx).Trim();
            var version = NuGetVersion.Parse(line.Substring(separatorIdx + 1).Trim());
            installedPackages.Add((id, version));
        }
    }
}

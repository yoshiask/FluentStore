using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chocolatey.Models;
using CliWrap;
using NuGet.Versioning;

namespace Chocolatey;

public class ChocoCliClient : IChocoPackageService
{
    private const string CHOCO_EXE = "choco";
    private const string CHOCO_PARAM_YES = "-y";
    private const string CHOCO_PARAM_LIMITOUTPUT = "--limit-output";

    private static readonly Regex _rxProgress = new(@"Progress: Downloading (?<id>[\w.\-_]+) (?<ver>\d+(\.\d+){0,3}(-[\w\d]+)?)\.\.\. (?<prog>\d{1,3})%", RegexOptions.Compiled);

    public bool NoOp { get; set; } = false;
    
    public async Task<bool> InstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null)
    {
        List<string> args = ["install", id, CHOCO_PARAM_YES, CHOCO_PARAM_LIMITOUTPUT];
        
        if (version is not null)
            args.Add($"--version=\"'{version}'\"");
        
        if (NoOp)
            args.Add("--noop");
        
        var result = await Cli.Wrap(CHOCO_EXE)
            .WithArguments(args)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(HandleStdOut))
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
        throw new NotImplementedException();
    }

    public async Task<bool> UpgradeAllAsync(IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpgradeAsync(string id, IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }
}

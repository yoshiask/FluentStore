using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Chocolatey.Models;
using NuGet.Versioning;

namespace Chocolatey.Cli;

public class ChocoAdminCliClient : IChocoPackageService
{
    public bool NoOp { get; set; } = false;

    public async Task<bool> InstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null)
    {
        return await Task.Run(delegate
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

            ProcessStartInfo info = new(ChocoArgumentsBuilder.CHOCO_EXE)
            {
                Arguments = string.Join(" ", args),

                // Required to run as admin
                UseShellExecute = true,
                Verb = "runas",
            };

            var process = Process.Start(info);
            process.WaitForExit();

            return process.ExitCode is 0;
        });
    }

    public Task<bool> UninstallAsync(string id, NuGetVersion? version = null, IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpgradeAllAsync(IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpgradeAsync(string id, IProgress<PackageProgress>? progress = null)
    {
        throw new NotImplementedException();
    }
}

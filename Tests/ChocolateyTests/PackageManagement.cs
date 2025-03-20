using System;
using System.Threading.Tasks;
using Chocolatey;
using Chocolatey.Models;
using Xunit;
using Xunit.Abstractions;

namespace ChocolateyTests;

public class PackageManagement(ITestOutputHelper output)
{
    private readonly IChocoPackageService _pkgMan = new ChocoCliClient
    {
        NoOp = true
    };

    [Theory]
    [InlineData("git")]
    [InlineData("dotnet-windowshosting")]
    [InlineData("notepadplusplus")]
    public async Task InstallLatestAsync(string id)
    {
        Progress<PackageProgress> progress = new(p => output.WriteLine(p.ToString()));
        await _pkgMan.InstallAsync(id, progress: progress);
    }

    [Theory]
    [InlineData("git")]
    [InlineData("dotnet-windowshosting")]
    public async Task UninstallLatestAsync(string id)
    {
        Progress<PackageProgress> progress = new(p => output.WriteLine(p.ToString()));
        await _pkgMan.UninstallAsync(id, progress: progress);
    }
}
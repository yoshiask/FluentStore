using Humanizer;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using PublishTool.Support;
using Spectre.Console;

namespace PublishTool.Commands;

public static class Release
{
    private static readonly string[] _architectures = ["x64", "x86"];

    public static async Task PublishAsync(CommandLineParser argParser)
    {
        var repoPath = argParser.GetArgument("-repo") ?? @"E:\Repos\yoshiask\FluentStore";
        var ipfsRootDir = argParser.GetArgument("-root") ?? @"E:\Documents\site\ipfs_test";
        var ipfsKeyName = argParser.GetArgument("-ipfsKey") ?? "fluent-store";
        var configuration = argParser.GetArgument("-configuration") ?? "Release";
        bool verbose = argParser.HasArgument("v");

        var appVersion = FluentStore.SDK.Plugins.NuGet.FluentStoreNuGetProject.CurrentSdkVersion;
        var plainAppVersion = appVersion.Version;
        var releaseTag = appVersion.Release.Titleize();

        var ipfsInstallerDir = Path.Combine(ipfsRootDir, "FluentStore", $"{releaseTag}Installer");
        var ipfsVersionedInstallerDir = Path.Combine(ipfsInstallerDir, plainAppVersion.ToString(3));

        MSBuildLocator.RegisterDefaults();

        var msixBundlePath = await BuildMsixBundle(repoPath, ipfsVersionedInstallerDir, plainAppVersion, releaseTag, configuration);
    }

    public static async Task<string?> BuildMsixBundle(string repoPath, string publishDir, Version appVersion, string releaseTag, string configuration)
    {
        var appCsprojPath = Path.Combine(repoPath, "FluentStore.App", "FluentStore.App.csproj");
        var versionStr = appVersion.ToString(4);
        Project appCsproj = new(appCsprojPath);

        var packageDir = new DirectoryInfo(Path.Combine(publishDir, "packs"));
        packageDir.Create();

        // Prepare project for building
        var projInstance = Microsoft.Build.Execution.BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(appCsproj);
        projInstance.SetProperty("Configuration", configuration);
        projInstance.SetProperty("GenerateAppxPackageOnBuild", "true");
        projInstance.SetProperty("AppxBundle", "Always");
        projInstance.SetProperty("AppxPackageDir", packageDir.FullName);
        // An architecture must be specified, but a bundle containing all architectures will be generated anyway
        projInstance.SetProperty("Platform", _architectures[0]);
        projInstance.SetProperty("RuntimeIdentifiers", $"win-{_architectures[0]}");

        AnsiConsoleMsbuildLogger logger = new(AnsiConsole.Console);
        var isSuccess = projInstance.Build(["Restore", "Build", "GenerateMsixPackage"], [logger]);
        if (!isSuccess)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Failed to build bundle[/]");
            return null;
        }

        var packageSrcFile = packageDir.EnumerateFiles(Path.Combine($"FluentStore.App_{versionStr}_Test", "*.msixbundle")).First();
        var packageDstPath = Path.Combine(packageDir.FullName, $"FluentStore{releaseTag}_{versionStr}.msixbundle");
        packageSrcFile.CopyTo(packageDstPath, true);

        AnsiConsole.MarkupLine($"[green]Built MSIX bundle at {packageDstPath}[/]");

        return packageDstPath;
    }
}

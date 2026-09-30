using CliWrap;
using Humanizer;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using PublishTool.Support;
using Spectre.Console;
using System.Text;

namespace PublishTool.Commands;

public static class Release
{
    private static readonly string[] _architectures = ["x64", "x86"];

    public static async Task PublishAsync(CommandLineParser argParser)
    {
        var repoPath = argParser.GetArgument("-repo") ?? @"E:\Repos\yoshiask\FluentStore";
        var ipfsRootDir = argParser.GetArgument("-root") ?? @"E:\Documents\site\ipfs_test";
        var pfxPath = argParser.GetArgument("-pfx") ?? @"E:\ssh-keys\yoshiask_self2025.pfx";
        var ipfsKeyName = argParser.GetArgument("-ipfsKey") ?? "askharoun";
        bool verbose = argParser.HasArgument("v");

        var appVersion = FluentStore.SDK.Plugins.NuGet.FluentStoreNuGetProject.CurrentSdkVersion;
        var plainAppVersion = appVersion.Version;
        var release = appVersion.Release.Titleize();

        var ipfsInstallerDir = Path.Combine(ipfsRootDir, "FluentStore", $"{release}Installer");
        var ipfsVersionedInstallerDir = Path.Combine(ipfsInstallerDir, plainAppVersion.ToString(3));

        MSBuildLocator.RegisterDefaults();

        var msixBundlePath = await BuildMsixBundle(repoPath, ipfsVersionedInstallerDir, plainAppVersion, release);
    }

    public static async Task<bool> BuildMsixBundle(string repoPath, string publishDir, Version appVersion, string release)
    {
        var appCsprojPath = Path.Combine(repoPath, "FluentStore.App", "FluentStore.App.csproj");
        var versionStr = appVersion.ToString(4);
        Project appCsproj = new(appCsprojPath);

        // Prepare project for building
        var templateProjInstance = Microsoft.Build.Execution.BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(appCsproj);
        var configuration = "Release";
        templateProjInstance.SetProperty("Configuration", configuration);
        templateProjInstance.SetProperty("GenerateAppxPackageOnBuild", "true");

        var packageDir = Path.Combine(publishDir, "packs");
        Directory.CreateDirectory(packageDir);
        AnsiConsoleMsbuildLogger logger = new();

        // Build MSIX for each architecture
        foreach (var arch in _architectures)
        {
            var projInstance = templateProjInstance.DeepCopy();
            projInstance.SetProperty("Platform", arch.ToString());
            projInstance.SetProperty("RuntimeIdentifiers", $"win-{arch}");

            var isSuccess = projInstance.Build(targets: ["restore", "build"], [logger]);
            if (!isSuccess)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Failed to build package for {arch}[/]");
                return false;
            }

            var packageSrcPath = Directory.GetFiles(
                Path.Combine(projInstance.Directory, "bin", arch, configuration, "*", "AppPackages", $"FluentStore.App_{versionStr}_Test", "*.msix")
            ).First();
            var packageDstPath = Path.Combine(packageDir, $"FluentStore{release}_{versionStr}_{arch}.msix");
            File.Copy(packageSrcPath, packageDstPath, true);

            AnsiConsole.MarkupLine($"Built MSIX package, copied to {packageDstPath}");
        }

        // Create MSIX bundle

        throw new NotImplementedException();
    }

    public static async Task<bool> BuildMsixBundleViaCli(string repoPath, string publishDir, Version appVersion, string release)
    {
        var appCsprojPath = Path.Combine(repoPath, "FluentStore.App", "FluentStore.App.csproj");
        var versionStr = appVersion.ToString(4);

        // Locate msbuild
        var msbuildPath = await LocateToolAsync("msbuild");

        // Build MSIX for each architecture
        var packageDir = Path.Combine(publishDir, "packs");
        Directory.CreateDirectory(packageDir);
        var configuration = "Release";
        foreach (var arch in _architectures)
        {
            var buildCmd = Cli.Wrap(msbuildPath)
                .WithArguments($"'{appCsprojPath}' /t:restore,build /p:Configuration={configuration} /p:Platform={arch} /p:RuntimeIdentifiers=win-{arch} /p:GenerateAppxPackageOnBuild=true")
                .WithStandardOutputPipe(ToConsole());

            var buildResult = await buildCmd.ExecuteAsync();
            if (!buildResult.IsSuccess)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Failed to build package for {arch}[/]");
                return false;
            }

            var packageSrcPath = Directory.GetFiles(
                Path.Combine(repoPath, "FluentStore.App", "bin", arch, configuration, "*", "AppPackages", $"FluentStore.App_{versionStr}_Test", "*.msix")
            ).First();
            var packageDstPath = Path.Combine(publishDir, $"FluentStore{release}_{versionStr}_{arch}.msix");
            File.Copy(packageSrcPath, packageDstPath, true);

            AnsiConsole.MarkupLine($"Built MSIX package, copied to {packageDstPath}");
        }

        // Create MSIX bundle

        throw new NotImplementedException();
    }

    private static async Task<string> LocateToolAsync(string name)
    {
        var whereOutput = new StringBuilder();
        var whereCmd = Cli.Wrap("where.exe")
            .WithArguments(name)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(whereOutput));

        await whereCmd.ExecuteAsync();

        return whereOutput.ToString()
            .Split('\n', StringSplitOptions.TrimEntries)
            .First();
    }

    private static PipeTarget ToConsole() => PipeTarget.ToDelegate(AnsiConsole.WriteLine);
}

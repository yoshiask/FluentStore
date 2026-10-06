using Humanizer;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using NuGet.Versioning;
using PublishTool.Support;
using Spectre.Console;

namespace PublishTool.Commands;

public class Release
{
    private static readonly string[] _architectures = ["x64", "x86"];

    private readonly IAnsiConsole _console;
    private readonly string _repoPath;
    private readonly string _ipfsRootDir;
    private readonly string _ipfsKeyName;
    private readonly string _configuration;
    private readonly bool _verbose;

    private readonly NuGetVersion _appVersion;
    private readonly Version _plainAppVersion;
    private readonly string _releaseTag;

    private readonly string _versionedOutputPath;

    public Release(CommandLineParser argParser, IAnsiConsole console)
    {
        _console = console;

        _repoPath = argParser.GetArgument("-repo") ?? @"E:\Repos\yoshiask\FluentStore";
        _ipfsRootDir = argParser.GetArgument("-root") ?? @"E:\Documents\site\ipfs_test";
        _ipfsKeyName = argParser.GetArgument("-ipfsKey") ?? "fluent-store";
        _configuration = argParser.GetArgument("-configuration") ?? "Release";
        _verbose = argParser.HasArgument("v");

        _appVersion = FluentStore.SDK.Plugins.NuGet.FluentStoreNuGetProject.CurrentSdkVersion;
        _plainAppVersion = _appVersion.Version;
        _releaseTag = _appVersion.Release.Titleize();

        var ipfsInstallerDir = Path.Combine(_ipfsRootDir, "FluentStore", $"{_releaseTag}Installer");
        _versionedOutputPath = Path.Combine(ipfsInstallerDir, _plainAppVersion.ToString(3));
    }

    public async Task PublishAsync()
    {
        MSBuildLocator.RegisterDefaults();

        var msixBundlePath = await BuildMsixBundle();
    }

    public async Task<string?> BuildMsixBundle()
    {
        var appCsprojPath = Path.Combine(_repoPath, "FluentStore.App", "FluentStore.App.csproj");
        var versionStr = _plainAppVersion.ToString(4);
        Project appCsproj = new(appCsprojPath);

        var packageDir = new DirectoryInfo(Path.Combine(_versionedOutputPath, "packs"));
        packageDir.Create();

        // Prepare project for building
        var projInstance = Microsoft.Build.Execution.BuildManager.DefaultBuildManager.GetProjectInstanceForBuild(appCsproj);
        projInstance.SetProperty("Configuration", _configuration);
        projInstance.SetProperty("GenerateAppxPackageOnBuild", "true");
        projInstance.SetProperty("AppxBundle", "Always");
        projInstance.SetProperty("AppxPackageDir", packageDir.FullName);
        // An architecture must be specified, but a bundle containing all architectures will be generated anyway
        projInstance.SetProperty("Platform", _architectures[0]);
        projInstance.SetProperty("RuntimeIdentifiers", $"win-{_architectures[0]}");

        AnsiConsoleMsbuildLogger logger = new(_console)
        {
            Verbosity = _verbose
                ? Microsoft.Build.Framework.LoggerVerbosity.Diagnostic
                : Microsoft.Build.Framework.LoggerVerbosity.Normal,
        };

        var isSuccess = projInstance.Build(["Restore", "Build", "GenerateMsixPackage"], [logger]);
        if (!isSuccess)
        {
            _console.MarkupLineInterpolated($"[red]Failed to build bundle[/]");
            return null;
        }

        var packageSrcFile = packageDir.EnumerateFiles(Path.Combine($"FluentStore.App_{versionStr}_Test", "*.msixbundle")).First();
        var packageDstPath = Path.Combine(packageDir.FullName, $"FluentStore{_releaseTag}_{versionStr}.msixbundle");
        packageSrcFile.CopyTo(packageDstPath, true);

        _console.MarkupLine($"[green]Built MSIX bundle at {packageDstPath}[/]");

        return packageDstPath;
    }
}

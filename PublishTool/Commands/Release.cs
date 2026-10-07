using FluentStore.SDK.Models;
using Humanizer;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Msix.Utils.AppxPackaging;
using Microsoft.Msix.Utils.AppxPackagingInterop;
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
    private readonly string _dependenciesOutputPath;

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
        _dependenciesOutputPath = Path.Combine(_ipfsRootDir, "Dependencies");
    }

    public async Task PublishAsync()
    {
        // Build and sign the main bundle
        var msixBundlePath = @"E:\Documents\site\ipfs_test\FluentStore\AlphaInstaller\0.4.2\packs\FluentStoreAlpha_0.4.2.0.msixbundle";
        //var msixBundlePath = await BuildMsixBundle();
        if (msixBundlePath is null)
            return;

        // Locate MSIX dependencies
        HashSet<PackageIdentity> dependencies = [.. GetMsixBundleDependencies(msixBundlePath)];

        // Pack plugins
        Plugin pluginCommand = new(_console,
            pluginId: null,
            repoPath: _repoPath,
            configuration: _configuration,
            verbose: _verbose,
            saveLogs: false,
            install: false,
            force: true);
        var pluginsPackedSuccessfully = await pluginCommand.BuildPluginsAsync();
        if (!pluginsPackedSuccessfully)
            return;
    }

    public async Task<string?> BuildMsixBundle()
    {
        MSBuildLocator.RegisterDefaults();

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

    public IEnumerable<PackageIdentity> GetMsixBundleDependencies(string msixBundlePath)
    {
        var metadata = new AppxBundleMetadata(msixBundlePath);
        var appxFactory = (IAppxFactory)new AppxFactory();

        // The APPX enumerator COM interfaces behave differently than C#'s `IEnumerator`.
        // For reference, see https://github.com/microsoft/MSIX-Toolkit/blob/ec2244a54530c3173e6e5a93ec1a2a525c8c6aeb/AppInstallerFileBuilder/AppInstallerFileBuilderLib/AppxPackaging/AppxMetadata.cs#L60-L68

        var packageEnumerator = metadata.AppxBundleReader.GetPayloadPackages();
        while (packageEnumerator.GetHasCurrent())
        {
            var package = packageEnumerator.GetCurrent();
            var packageReader = appxFactory.CreatePackageReader(package.GetStream());
            var packageManifest = packageReader.GetManifest();

            foreach (var dependency in GetMsixDependencies(packageManifest))
                yield return dependency;

            packageEnumerator.MoveNext();
        }
    }

    public IEnumerable<PackageIdentity> GetMsixDependencies(IAppxManifestReader packageManifest)
    {
        var packageArchitecture = GetArchitectureFromAppx(packageManifest.GetPackageId().GetArchitecture());

        var dependencyEnumerator = packageManifest.GetPackageDependencies();
        while (dependencyEnumerator.GetHasCurrent())
        {
            var packageDependency = dependencyEnumerator.GetCurrent();
            var packageVersion = GetVersionFromULong(packageDependency.GetMinVersion());

            yield return new PackageIdentity(
                packageDependency.GetName(),
                packageVersion,
                packageArchitecture,
                packageDependency.GetPublisher());

            dependencyEnumerator.MoveNext();
        }
    }

    private static Version GetVersionFromULong(ulong value)
    {
        var major = (int)(value >> 48) & 0xFFFF;
        var minor = (int)(value >> 32) & 0xFFFF;
        var build = (int)(value >> 16) & 0xFFFF;
        var revision = (int)value & 0xFFFF;
        return new(major, minor, build, revision);
    }

    private static Architecture GetArchitectureFromAppx(APPX_PACKAGE_ARCHITECTURE architecture)
    {
        return architecture switch
        {
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_X86 or
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_X86A64 => Architecture.x86,
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_X64 => Architecture.x64,
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_ARM => Architecture.Arm,
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_ARM64 => Architecture.Arm64,
            APPX_PACKAGE_ARCHITECTURE.APPX_PACKAGE_ARCHITECTURE_NEUTRAL => Architecture.Neutral,
            _ => Architecture.Unknown
        };
    }
}

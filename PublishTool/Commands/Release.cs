using FluentStore.SDK.Models;
using Humanizer;
using Ipfs;
using Ipfs.CoreApi;
using Ipfs.Http;
using Meziantou.Framework;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.Msix.Utils.AppxPackaging;
using Microsoft.Msix.Utils.AppxPackagingInterop;
using NuGet.Packaging;
using NuGet.Versioning;
using PublishTool.Support;
using Spectre.Console;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PublishTool.Commands;

public partial class Release
{
    private static readonly string[] _architectures = ["x64", "x86"];
    private const string _fluentstoreIpns = "fluentstore.askharoun.com";

    private readonly IAnsiConsole _console;
    private readonly string _repoDir;
    private readonly PathRoot _outRootDir;
    private readonly string _ipfsKeyName;
    private readonly string _configuration;
    private readonly bool _verbose;

    private readonly NuGetVersion _appVersion;
    private readonly Version _plainAppVersion;
    private readonly string _releaseTag;

    private readonly PathRoot _ipfsPath = new($"/ipfs", '/');
    private readonly PathRoot _fluentstoreIpnsPath = new($"/ipns/{_fluentstoreIpns}", '/');
    private readonly PathRoot _ipfsGatewayRootUri = new Uri("https://ipfs.askharoun.com");
    private readonly PathRoot _dependenciesOutputPath;

    private readonly PathFragment _taggedOutputPath;
    private readonly PathFragment _versionedOutputPath;

    private readonly HashSet<string> _ipfsPathsToPin = [];

    public Release(CommandLineParser argParser, IAnsiConsole console)
    {
        _console = console;

        _repoDir = argParser.GetArgument("-repo") ?? @"E:\Repos\yoshiask\FluentStore";
        _outRootDir = argParser.GetArgument("-root") ?? @"E:\Documents\site\ipfs_test\FluentStore";
        _ipfsKeyName = argParser.GetArgument("-ipfsKey") ?? "fluent-store";
        _configuration = argParser.GetArgument("-configuration") ?? "Release";
        _verbose = argParser.HasArgument("v");

        _appVersion = FluentStore.SDK.Plugins.NuGet.FluentStoreNuGetProject.CurrentSdkVersion;
        _plainAppVersion = _appVersion.Version;
        _releaseTag = _appVersion.Release.Titleize();

        _dependenciesOutputPath = _outRootDir / "Dependencies";

        _taggedOutputPath = $"{_releaseTag}Installer";
        _versionedOutputPath = _taggedOutputPath / _plainAppVersion.ToString(3);
    }

    public async Task PublishAsync()
    {
        // Build and sign the main bundle
        MSBuildLocator.RegisterDefaults();
        var msixBundlePath = @"E:\Documents\site\ipfs_test\FluentStore\AlphaInstaller\0.4.2\FluentStoreAlpha_0.4.2.0.msixbundle";
        //var msixBundlePath = await BuildMsixBundle();
        if (msixBundlePath is null)
            return;

        IpfsClient ipfsClient = new();

        // Pin main MSIX bundle to IPFS. The `.appinstaller` file needs all the CIDs ahead of time
        AppxBundleMetadata mainBundleMetadata = new(msixBundlePath);
        var mainBundleCid = await _console.Status().StartAsync("Pinning MSIX bundle file to IPFS...", async ctx =>
        {
            var mainBundleIpfsNode = await ipfsClient.FileSystem.AddFileAsync(msixBundlePath, new AddFileOptions
            {
                PinName = $"{mainBundleMetadata.PackageFullName}.msixbundle",
                Progress = new Progress<TransferProgress>(p => ctx.Status(new ByteSize(p.Bytes).Humanize()))
            });
            return mainBundleIpfsNode.Id;
        });
        var mainBundleIpfsPath = _ipfsPath / mainBundleCid;
        _ipfsPathsToPin.Add(mainBundleIpfsPath);
        _console.MarkupLineInterpolated($"[green]Pinned main bundle as {mainBundleCid}[/]");

        // Identify and pin MSIX dependencies to IPFS
        var dependencies = await PinMsixDependencies(ipfsClient, mainBundleMetadata);

        // Write `.appinstaller` file
        var appInstallerPath = await WriteAppInstallerAsync(mainBundleMetadata, mainBundleIpfsPath, dependencies);

        return;

        // Pack plugins
        Plugin pluginCommand = new(_console,
            pluginId: null,
            repoPath: _repoDir,
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
        var appCsprojPath = Path.Combine(_repoDir, "FluentStore.App", "FluentStore.App.csproj");
        var versionStr = _plainAppVersion.ToString(4);
        Project appCsproj = new(appCsprojPath);

        var packageDir = new DirectoryInfo(_outRootDir / _versionedOutputPath);
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

    public IEnumerable<PackageIdentity> GetMsixBundleDependencies(IAppxBundleReader bundleReader)
    {
        var appxFactory = (IAppxFactory)new AppxFactory();

        // The APPX enumerator COM interfaces behave differently than C#'s `IEnumerator`.
        // For reference, see https://github.com/microsoft/MSIX-Toolkit/blob/ec2244a54530c3173e6e5a93ec1a2a525c8c6aeb/AppInstallerFileBuilder/AppInstallerFileBuilderLib/AppxPackaging/AppxMetadata.cs#L60-L68

        var packageEnumerator = bundleReader.GetPayloadPackages();
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

    public async Task<Dictionary<PackageIdentity, string>> PinMsixDependencies(IpfsClient ipfsClient, AppxBundleMetadata mainBundleMetadata)
    {
        Dictionary<PackageIdentity, string> dependencies = [];

        PinListOptions pinListOptions = new() { Names = true, Stream = true, Type = PinType.Recursive };
        Dictionary<PackageFullName, Cid> existingPinnedDependencies = [];
        await foreach (var pin in ipfsClient.Pin.ListAsync(pinListOptions))
        {
            var pinName = pin.Name;
            if (pinName is null)
                continue;

            if (pinName.EndsWith(".appx") || pinName.EndsWith(".msix"))
                pinName = pinName[..^5];

            if (!PackageFullName.TryParse(pinName, out var pfn))
                continue;

            existingPinnedDependencies[pfn] = pin.Cid;
        }

        foreach (var dep in GetMsixBundleDependencies(mainBundleMetadata.AppxBundleReader))
        {
            _console.WriteLine($"Dependency {dep}:");

            var depPfn = dep.ToPackageFullName(string.Empty);

            if (existingPinnedDependencies.TryGetValue(depPfn, out var depCid))
            {
                dependencies[dep] = depCid;
                _console.MarkupLineInterpolated($"    [green]Already pinned as {depCid}.[/]");
                continue;
            }

            var depMsixPath = Directory
                .EnumerateFiles(_outRootDir / _versionedOutputPath, "*", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.EndsWith($"\\{dep.Architecture}\\{dep.Name}.msix"));
            if (depMsixPath is null)
            {
                _console.MarkupLine("    [yellow]Could not locate.[/]");
                var depCidStr = await _console.AskAsync<string>($"CID? ");
                dependencies[dep] = depCidStr;
                continue;
            }

            _console.MarkupLine("    [green]Found dependency file.[/]");

            var ipfsNode = await _console.Status().StartAsync("    Pinning dependency MSIX file to IPFS...", async ctx =>
            {
                return await ipfsClient.FileSystem.AddFileAsync(depMsixPath, new AddFileOptions
                {
                    PinName = $"{depPfn}.msix",
                    Progress = new Progress<TransferProgress>(p => ctx.Status(new ByteSize(p.Bytes).Humanize()))
                });
            });

            dependencies[dep] = _ipfsPath / ipfsNode.Id;

            _console.MarkupLineInterpolated($"    [green]Pinned as {ipfsNode.Id}.[/]");
        }

        _ipfsPathsToPin.AddRange(dependencies.Values);
        return dependencies;
    }

    public async Task<string?> WriteAppInstallerAsync(AppxBundleMetadata mainBundleMetadata, string mainBundleIpfsPath, IDictionary<PackageIdentity, string> dependencies)
    {
        var appInstallerPath = _taggedOutputPath / $"FluentStore{_releaseTag}.appinstaller";

        XDocument xDoc = new();
        {
            XNamespace nsAppInstaller = XNamespace.Get("http://schemas.microsoft.com/appx/appinstaller/2017");
            XNamespace nsFluent2610 = XNamespace.Get("http://fluentstore.askharoun.com/appx/appinstaller/2610");

            XElement xAppInstaller = new(nsAppInstaller + "AppInstaller",
                new XAttribute("xmlns", nsAppInstaller),
                new XAttribute(XNamespace.Xmlns + "fluent2610", nsFluent2610));

            xAppInstaller.SetAttributeValue("Version", mainBundleMetadata.Version);
            xAppInstaller.SetAttributeValue("Uri", _ipfsGatewayRootUri / (_fluentstoreIpnsPath / appInstallerPath));
            xAppInstaller.SetAttributeValue(nsFluent2610 + "IpfsPath", _fluentstoreIpnsPath / appInstallerPath);
            xDoc.Add(xAppInstaller);

            XElement xMainPackage = new(nsAppInstaller + "MainBundle");
            xMainPackage.SetAttributeValue("Name", mainBundleMetadata.PackageName);
            xMainPackage.SetAttributeValue("Publisher", mainBundleMetadata.Publisher);
            xMainPackage.SetAttributeValue("Version", mainBundleMetadata.Version);
            xMainPackage.SetAttributeValue("Uri", _ipfsGatewayRootUri / mainBundleIpfsPath);
            xMainPackage.SetAttributeValue(nsFluent2610 + "IpfsPath", mainBundleIpfsPath);
            xAppInstaller.Add(xMainPackage);

            XElement xDependencies = new(nsAppInstaller + "Dependencies");
            xAppInstaller.Add(xDependencies);
            foreach ((var dependency, var depCid) in dependencies)
            {
                var depIpfsPath = _ipfsPath / depCid;
                XElement xDependency = new(nsAppInstaller + "Package");
                xDependency.SetAttributeValue("Name", dependency.Name);
                xDependency.SetAttributeValue("Version", dependency.Version);
                xDependency.SetAttributeValue("ProcessorArchitecture", dependency.Architecture);
                xDependency.SetAttributeValue("Publisher", dependency.Publisher);
                xDependency.SetAttributeValue("Uri", _ipfsGatewayRootUri / depIpfsPath);
                xDependency.SetAttributeValue(nsFluent2610 + "IpfsPath", depIpfsPath);
                xDependencies.Add(xDependency);
            }
        }

        // TODO: Add custom element declaring dependency on .NET runtime

        var appInstallerFilePath = _outRootDir / appInstallerPath;
        await using FileStream stream = new(appInstallerFilePath, FileMode.Create);
        await using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = true,
            Indent = true,
            IndentChars = "    ",
            NewLineOnAttributes = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Encoding = Encoding.UTF8,
        });
        await xDoc.WriteToAsync(xmlWriter, default);

        _console.MarkupLineInterpolated($"[green]Generated App Installer file at {appInstallerFilePath}[/]");

        return appInstallerFilePath;
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

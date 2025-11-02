using CommunityToolkit.Diagnostics;
using FluentStore.SDK;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Plugins.NuGet;
using Humanizer;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.Helpers.Updater;

internal partial class AppUpdatePackage : PackageBase<OnlineVersionInfo>
{
    public AppUpdatePackage(PackageHandlerBase packageHandler, OnlineVersionInfo versionInfo, string release, OnlineInstallerInfo installer) : base(packageHandler)
    {
        Guard.IsNotNull(versionInfo, nameof(versionInfo));
        Guard.IsNotNull(installer, nameof(installer));

        Release = release;
        Model = versionInfo;

        Urn = AppUpdatePackageSource.FormatUrn(release);
        ShortTitle = App.AppName;
        Title = $"{App.AppName} {release.Titleize()}";
        Version = Model.VersionStr;
        Description = Model.Description;
        DeveloperName = "Joshua Askharoun";
        PublisherId = "yoshiask";
        
        InstallerInfo = installer;
        Type = InstallerInfo.Type;

        Status = PackageStatus.Details;
    }

    public OnlineInstallerInfo InstallerInfo { get; private set; }

    public string Release { get; private set; }

    public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(new FileImage("ms-appx:///Assets/AppIcon.ico"));

    public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

    public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult<List<ImageBase>>([]);

    public override Task<bool> CanDownloadAsync() => Task.FromResult(InstallerInfo?.IsValid() ?? false);

    public override Task<string> GetCannotBeInstalledReason()
    {
        if (!InstallerInfo?.IsValid() ?? false)
            return Task.FromResult("Installer information is either unavailable or does not provide any sources to download from.");

        var onlineVersion = Model.Version;
        var installedVersion = FluentStoreNuGetProject.CurrentSdkVersion;
        if (onlineVersion <= installedVersion)
            return Task.FromResult($"Detected app downgrade. Version {onlineVersion} is older than the installed version {installedVersion}.");

        return Task.FromResult<string>(null);
    }

    public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

    public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
    {
        folder ??= StorageHelper.GetTempDirectory().CreateSubdirectory(StorageHelper.PrepUrnForFile(Urn));

        IFile srcFile = InstallerInfo switch
        {
            { Cid: not null } => new IpfsFile(InstallerInfo.Cid, AbstractStorageHelper.IpfsClient),
            { HttpUrl: not null } => AbstractStorageHelper.GetFileFromUrl(InstallerInfo.HttpUrl),

            _ => throw new InvalidProgramException($"{Title} does not specify any supported files to download.")
        };

        var fileName = srcFile.Name;
        if (!Path.HasExtension(fileName))
            fileName = $"FluentStoreBeta_{Version}.{InstallerInfo.Type.ToString().ToLowerInvariant()}";

        SystemFolder dstDir = new(folder);
        var dstFile = await dstDir.CreateCopyOfAsync(srcFile, true, fileName);

        DownloadItem = ((SystemFile)dstFile).Info;
        return DownloadItem;
    }

    public override Task<bool> InstallAsync()
    {
        // Can't use Fluent Store to install an update for Fluent Store
        throw new NotImplementedException();
    }

    public override Task LaunchAsync() => throw new NotImplementedException();
}

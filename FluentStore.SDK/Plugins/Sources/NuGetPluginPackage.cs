using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources;

public partial class NuGetPluginPackage : PluginPackageBase
{
    private Uri _iconUri = new("https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/240px-NuGet_project_logo.svg.png");
    private DownloadResourceResult _downloadItem;

    [ObservableProperty]
    private string _nuGetId;

    public NuGetPluginPackage(NuGetPluginHandler packageHandler, IPackageSearchMetadata searchMetadata = null, NuspecReader nuspec = null, PackageReaderBase reader = null) : base(packageHandler, packageHandler.PluginLoader)
    {
        if (searchMetadata is not null)
            Update(searchMetadata);
        if (nuspec is not null)
            Update(nuspec);

        if (reader is not null)
            _downloadItem = new(reader, "");
    }

    public override async Task<ImageBase> CacheAppIcon() => _iconUri is null ? null : new FileImage(_iconUri);

    public override async Task<bool> CanDownloadAsync() => Urn is not null && Version is not null;

    public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
    {
        DownloadCache cache = new(folder);

        if (cache.TryGetFile(Urn, Version, out var file))
        {
            IsDownloaded = true;
            return file;
        }

        var downloadItem = await GetResourceAsync();
        var nupkgFile = StorageHelper.GetPackageFile(Urn, folder);

        await downloadItem.PackageReader.CopyNupkgAsync(nupkgFile.FullName, default);
        cache.Add(Urn, Version, nupkgFile);

        IsDownloaded = true;
        return nupkgFile;
    }

    public override async Task<bool> InstallAsync()
    {
        DownloadResourceResult downloadItem;
        try
        {
            WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(this));
            downloadItem = await GetResourceAsync();
            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPluginDownloadCompleted(NuGetId));
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PluginDownloadFailed));
            return false;
        }

        PluginInstallStatus status = PluginInstallStatus.Failed;
        try
        {
            WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(this));
            status = await _pluginLoader.InstallPlugin(downloadItem, true);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PluginInstallFailed));
        }

        DisposeResource();

        if (status.IsGreaterThan(PluginInstallStatus.Failed))
        {
            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPluginInstallCompleted(NuGetId));
            IsInstalled = true;
            return true;
        }

        return false;
    }

    public override ImageBase GetStatusImage()
    {
        if (_pluginLoader.Project.Entries.TryGetValue(NuGetId, out var entry)
            && !_pluginLoader.Project.CheckCompatibility(entry.Framework, entry.SdkVersion))
        {
            // Installed plugin version is not compatible with this version of the SDK
            TextImage icon = new()
            {
                FontFamily = SharedResources.SymbolFont,
                Text = "\uE7BA",
                ForegroundColor = SharedResources.WarningColor,
            };
            return icon;
        }

        return base.GetStatusImage();
    }

    public void Update(IPackageSearchMetadata searchMetadata)
    {
        NuGetId = searchMetadata.Identity.Id;
        Title = searchMetadata.Title ?? searchMetadata.Identity.Id;
        Description = searchMetadata.Description;
        DeveloperName = searchMetadata.Authors;
        Version = searchMetadata.Identity.Version?.OriginalVersion;

        if (searchMetadata.IconUrl is not null)
            _iconUri = searchMetadata.IconUrl;

        Update();
    }

    public void Update(NuspecReader nuspec)
    {
        NuGetId = nuspec.GetId();
        Title = nuspec.GetTitle();
        Description = nuspec.GetDescription();
        DeveloperName = nuspec.GetAuthors();
        Version = nuspec.GetVersion().ToString();

        var iconUrl = nuspec.GetIconUrl();
        if (!string.IsNullOrEmpty(iconUrl))
            _iconUri = new(iconUrl);

        if (string.IsNullOrEmpty(Title))
            Title = nuspec.GetId();

        Update();
    }

    private void Update()
    {
        Urn ??= new(NuGetPluginHandler.NAMESPACE_NUGETPLUGIN, new RawNamespaceSpecificString(NuGetId));

        if (_pluginLoader.Project.Entries.TryGetValue(NuGetId, out var pluginEntry))
        {
            IsInstalled = true;
            InstalledVersion = pluginEntry.Version.ToFullString();
        }
    }

    private async Task<DownloadResourceResult> GetResourceAsync()
    {
        if (_downloadItem is not null)
            return _downloadItem;

        return _downloadItem = await _pluginLoader.FetchPlugin(NuGetId);
    }

    private void DisposeResource()
    {
        _downloadItem?.Dispose();
        _downloadItem = null;
    }
}

﻿using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources;

public partial class NuGetPluginPackage : PackageBase
{
    private readonly PluginLoader _pluginLoader;

    private Uri _iconUri = new("https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/240px-NuGet_project_logo.svg.png");
    private DownloadResourceResult _downloadItem;

    [ObservableProperty]
    private string _nuGetId;

    public NuGetPluginPackage(NuGetPluginHandler packageHandler, IPackageSearchMetadata searchMetadata = null, NuspecReader nuspec = null, PackageReaderBase reader = null) : base(packageHandler)
    {
        _pluginLoader = packageHandler.PluginLoader;

        if (searchMetadata is not null)
            Update(searchMetadata);
        if (nuspec is not null)
            Update(nuspec);

        if (reader is not null)
            _downloadItem = new(reader, "");
    }

    public override async Task<ImageBase> CacheAppIcon() => _iconUri is null ? null : new FileImage(_iconUri);

    public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

    public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult<List<ImageBase>>([]);

    public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

    public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
    {
        DownloadCache cache = new(folder);

        if (cache.TryGetFile(Urn, Version, out var file))
        {
            Status = PackageStatus.Downloaded;
            return file;
        }

        var downloadItem = await GetResourceAsync();
        var nupkgFile = StorageHelper.GetPackageFile(Urn, folder);

        await downloadItem.PackageReader.CopyNupkgAsync(nupkgFile.FullName, default);
        cache.Add(Urn, Version, nupkgFile);

        Status = PackageStatus.Downloaded;
        return nupkgFile;
    }

    public override async Task<bool> InstallAsync()
    {
        var downloadItem = await GetResourceAsync();
        var status = await _pluginLoader.InstallPlugin(downloadItem, true);

        DisposeResource();

        if (status.IsGreaterThan(PluginInstallStatus.Failed))
        {
            Status = PackageStatus.Installed;
            return true;
        }

        return false;
    }

    public override Task LaunchAsync()
    {
        throw new NotImplementedException();
    }

    public void Update(IPackageSearchMetadata searchMetadata)
    {
        NuGetId = searchMetadata.Identity.Id;
        Title = searchMetadata.Title ?? searchMetadata.Identity.Id;
        Description = searchMetadata.Description;
        DeveloperName = searchMetadata.Authors;
        Version = searchMetadata.Identity.Version?.OriginalVersion;
        Urn ??= new(NuGetPluginHandler.NAMESPACE_NUGETPLUGIN, new RawNamespaceSpecificString(NuGetId));

        if (searchMetadata.IconUrl is not null)
            _iconUri = searchMetadata.IconUrl;
    }

    public void Update(NuspecReader nuspec)
    {
        NuGetId = nuspec.GetId();
        Title = nuspec.GetTitle();
        Description = nuspec.GetDescription();
        DeveloperName = nuspec.GetAuthors();
        Version = nuspec.GetVersion().ToString();
        Urn ??= new(NuGetPluginHandler.NAMESPACE_NUGETPLUGIN, new RawNamespaceSpecificString(NuGetId));

        var iconUrl = nuspec.GetIconUrl();
        if (!string.IsNullOrEmpty(iconUrl))
            _iconUri = new(iconUrl);

        if (string.IsNullOrEmpty(Title))
            Title = nuspec.GetId();
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

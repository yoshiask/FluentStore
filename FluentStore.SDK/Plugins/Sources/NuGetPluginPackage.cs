using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.SDK.Images;
using Flurl.Util;
using Garfoot.Utilities.FluentUrn;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources;

public partial class NuGetPluginPackage : PackageBase
{
    private Uri _iconUri = new("https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/240px-NuGet_project_logo.svg.png");
    private readonly PluginLoader _pluginLoader;

    [ObservableProperty]
    private string _nuGetId;

    public NuGetPluginPackage(NuGetPluginHandler packageHandler, IPackageSearchMetadata searchMetadata = null, NuspecReader nuspec = null) : base(packageHandler)
    {
        _pluginLoader = packageHandler.PluginLoader;

        if (searchMetadata is not null)
            Update(searchMetadata);
        if (nuspec is not null)
            Update(nuspec);
    }

    public override async Task<ImageBase> CacheAppIcon() => _iconUri is null ? null : new FileImage(_iconUri);

    public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

    public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult<List<ImageBase>>([]);

    public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

    public override Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> InstallAsync() => _pluginLoader.InstallPlugin(NuGetId, true);

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

    public void Update(IEnumerable<VersionInfo> versions)
    {
        Version = versions.OrderDescending().First().ToString();
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
}

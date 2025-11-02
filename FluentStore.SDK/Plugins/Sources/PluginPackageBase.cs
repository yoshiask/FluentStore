using CommunityToolkit.Mvvm.ComponentModel;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources;

public abstract partial class PluginPackageBase(PackageHandlerBase packageHandler, PluginLoader pluginLoader) : PackageBase(packageHandler)
{
    protected readonly PluginLoader _pluginLoader = pluginLoader;
    private ImageBase _statusImage;

    protected bool _cachedStatusImage;

    [ObservableProperty]
    private string _installedVersion;

    public ImageBase StatusImage
    {
        get
        {
            if (_cachedStatusImage)
                return _statusImage;

            _statusImage = GetStatusImage();
            _cachedStatusImage = true;

            return _statusImage;
        }
    }

    public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(TextImage.CreateFromName(ShortTitle ?? Title));

    public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

    public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult<List<ImageBase>>([]);

    public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

    public override Task LaunchAsync()
    {
        throw new NotImplementedException();
    }

    public virtual async Task<bool> UninstallAsync()
    {
        var status = await _pluginLoader.UninstallPlugin(Urn.GetContent());
        return status.IsAtLeast(PluginInstallStatus.NoAction);
    }

    public virtual bool IsUpdateAvailable(bool includePrerelease = false)
    {
        if (InstalledVersion is null)
            return false;

        var installed = global::NuGet.Versioning.NuGetVersion.Parse(InstalledVersion);
        var latest = global::NuGet.Versioning.NuGetVersion.Parse(Version);

        return latest > installed;
    }

    public virtual ImageBase GetStatusImage()
    {
        // TODO: We don't really want the backend model to handle
        // UI stuff like this. Probably better to use a status enum,
        // maybe with flags.
        
        if (IsUpdateAvailable())
        {
            return new TextImage
            {
                FontFamily = SharedResources.SymbolFont,
                Text = "\uECC5",
                ForegroundColor = SharedResources.InfoColor,
                Caption = $"An update is available to {Version}.",
            };
        }
        else if (IsInstalled)
        {
            return new TextImage
            {
                FontFamily = SharedResources.SymbolFont,
                Text = "\uE73E",
                ForegroundColor = SharedResources.SuccessColor,
                Caption = $"Version {InstalledVersion} is installed.",
            };
        }

        return null;
    }
}

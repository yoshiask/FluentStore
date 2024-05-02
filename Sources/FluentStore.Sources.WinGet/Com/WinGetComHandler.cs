using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Extensions.Logging;
using Microsoft.Management.Deployment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinGet.Sharp;

namespace FluentStore.Sources.WinGet.Com;

internal class WinGetComHandler : IWinGetImplementation
{
    private readonly PackageManager _manager;
    private readonly PackageCatalog _catalog;

    private WinGetComHandler(PackageManager manager, PackageCatalog catalog)
    {
        _manager = manager;
        _catalog = catalog;
    }

    public static async Task<WinGetComHandler> TryCreateAsync()
    {
        try
        {
            var manager = WinGetFactory.CreatePackageManager();

            var catalogRef = manager.GetPredefinedPackageCatalog(PredefinedPackageCatalog.OpenWindowsCatalog);
            var connection = await catalogRef.ConnectAsync();

            var catalog = connection.Status switch
            {
                ConnectResultStatus.Ok => connection.PackageCatalog,
                ConnectResultStatus.SourceAgreementsNotAccepted => throw new Exception("You must accept the source agreements."),
                _ => throw new Exception("Failed to connect to catalog."),
            };

            return new WinGetComHandler(manager, catalog);
        }
        catch (Exception ex)
        {
            var log = Ioc.Default.GetService<ILogger>();
            log?.LogError(ex, "Exception while creating WinGet COM handler");
            
            return null;
        }
    }

    public async Task<PackageBase> GetPackage(string id, WinGetProxyHandler packageHandler, PackageStatus status = PackageStatus.Details)
    {
        var filter = WinGetFactory.CreatePackageMatchFilter();
        filter.Field = PackageMatchField.Id;
        filter.Option = PackageFieldMatchOption.EqualsCaseInsensitive;
        filter.Value = id;

        var findOptions = WinGetFactory.CreateFindPackagesOptions();
        findOptions.Filters.Add(filter);

        var result = await _catalog.FindPackagesAsync(findOptions);
        if (result.Matches.Count < 1)
            return null;

        return CreateSDKPackage(packageHandler, result.Matches[0].CatalogPackage);
    }

    public async IAsyncEnumerable<PackageBase> SearchAsync(string query, WinGetProxyHandler packageHandler)
    {
        var filter = WinGetFactory.CreatePackageMatchFilter();
        filter.Field = PackageMatchField.Name;
        filter.Option = PackageFieldMatchOption.ContainsCaseInsensitive;
        filter.Value = query;

        var findOptions = WinGetFactory.CreateFindPackagesOptions();
        findOptions.Filters.Add(filter);

        var result = await _catalog.FindPackagesAsync(findOptions);

        foreach (var matchResult in result.Matches.ToList())
            yield return CreateSDKPackage(packageHandler, matchResult.CatalogPackage);
    }

    public Task<FileSystemInfo> DownloadAsync(WinGetPackage package, DirectoryInfo folder = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> InstallAsync(WinGetPackage package)
    {
        var comPackage = (CatalogPackage)package.Model;

        PackageInstallProgressState lastState = default;
        void InstallProgress(InstallProgress p)
        {
            if (p.State == PackageInstallProgressState.Downloading)
            {
                if (lastState.IsLessThan(p.State))
                    WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(package));

                WeakReferenceMessenger.Default.Send(new PackageDownloadProgressMessage(package, p.BytesDownloaded, p.BytesRequired));
            }
            else if (p.State == PackageInstallProgressState.Installing)
            {
                if (lastState.IsLessThan(p.State))
                {
                    WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(package));
                    WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(package));
                }

                WeakReferenceMessenger.Default.Send(new PackageInstallProgressMessage(package, p.InstallationProgress));
            }
        }

        WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(package));

        var options = WinGetFactory.CreateInstallOptions();
        var operation = _manager.InstallPackageAsync(comPackage, options);

        var result = await operation.AsTask(new Progress<InstallProgress>(InstallProgress));
        if (result.Status != InstallResultStatus.Ok)
        {
            string statusMessage = result.Status switch
            {
                InstallResultStatus.BlockedByPolicy => $"Installation of {package.Title} was blocked by an adminitrator policy.",
                InstallResultStatus.CatalogError => $"The {_catalog.Info.Name} catalog failed.",
                InstallResultStatus.InternalError => "An internal WinGet error occurred.",
                InstallResultStatus.InvalidOptions => "Invalid options were passed to WinGet. Please report this issue at https://github.com/yoshiask/FluentStore/issues.",
                InstallResultStatus.DownloadError => $"Could not download {package.Title}.",
                InstallResultStatus.InstallError => $"Could not install {package.Title}.",
                InstallResultStatus.ManifestError => $"{package.Title} provided an invalid manifest.",
                InstallResultStatus.NoApplicableInstallers => $"{package.Title} is not available for your system.",
                InstallResultStatus.NoApplicableUpgrade => $"A newer version of {package.Title} is already installed.",
                InstallResultStatus.PackageAgreementsNotAccepted => $"The agreements for {package.Title} have not been accepted.",
                _ => $"An unknown error occurred: {result.Status}."
            };

            ErrorType errorType = result.Status switch
            {
                InstallResultStatus.InvalidOptions or
                InstallResultStatus.CatalogError => ErrorType.PackageFetchFailed,

                InstallResultStatus.DownloadError => ErrorType.PackageDownloadFailed,

                _ => ErrorType.PackageInstallFailed,
            };

            Exception exception = new(statusMessage, result.ExtendedErrorCode);

            WeakReferenceMessenger.Default.Send(new ErrorMessage(exception, package, errorType));

            return false;
        }
        else
        {
            WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(package));
            return true;
        }
    }

    private static WinGetPackage CreateSDKPackage(WinGetProxyHandler packageHandler, CatalogPackage comPackage)
    {
        var metadata = comPackage.DefaultInstallVersion.GetCatalogPackageMetadata();

        WinGetPackage sdkPackage = new(packageHandler, comPackage)
        {
            WinGetId = comPackage.Id,
            Title = metadata.PackageName,
            DeveloperName = metadata.Publisher,
            Description = metadata.Description,
            Version = comPackage.DefaultInstallVersion.Version,
            Status = PackageStatus.Details
        };

        sdkPackage.Urn = Urn.Parse($"urn:{WinGetProxyHandler.NAMESPACE_WINGET}:{sdkPackage.WinGetId}");
        sdkPackage.PublisherId = sdkPackage.WinGetId.Split(['.'], 2)[0];
        sdkPackage.Website = Link.Create(metadata.PackageUrl, sdkPackage.ShortTitle + " website");
        sdkPackage.PrivacyUri = Link.Create(metadata.PrivacyUrl, sdkPackage.ShortTitle + " privacy policy");
        sdkPackage.SupportUrl = Link.Create(metadata.PublisherSupportUrl, sdkPackage.DeveloperName + " support");

        return sdkPackage;
    }
}

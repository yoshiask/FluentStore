using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinGet.Sharp;
using WinGet.Sharp.Models;

namespace FluentStore.Sources.Microsoft.WinGet.Cli;

internal class WinGetCliHandler : IWinGetImplementation
{
    readonly WGetNET.WinGetPackageManager _packageManager = new();

    public async Task<PackageBase> GetPackage(string id, WinGetProxyHandler packageHandler, PackageStatus status = PackageStatus.Details)
    {
        var results = await _packageManager.SearchPackageAsync(id, true);
        var result = results.FirstOrDefault(IsNotStoreSourced);

        if (result is null)
            return null;

        var package = CreateSDKPackage(packageHandler, result);

        if (status.IsAtLeast(PackageStatus.Details))
            await UpdateSDKPackage(package);

        return package;
    }

    public async IAsyncEnumerable<PackageBase> SearchAsync(string query, WinGetProxyHandler packageHandler)
    {
        var results = await _packageManager.SearchPackageAsync(query);
        foreach (var result in results.Where(IsNotStoreSourced))
            yield return CreateSDKPackage(packageHandler, result);
    }

    public Task<bool> CanDownloadAsync(PackageBase package, string id) => Task.FromResult(id is not null);

    public async Task<FileSystemInfo> DownloadAsync(PackageBase package, string id, DirectoryInfo folder)
    {
        WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(package));

        try
        {
            var downloaded = await _packageManager.DownloadAsync(id, folder);

            if (downloaded)
            {
                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(package));

                // Super-mega-hack because WGetNET doesn't tell us where the file is saved :facepalm:
                var fileNames = folder
                    .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Select(f => f.Name);

                var bestFileName = FuzzySharp.Process.ExtractOne(id, fileNames).Value;
                return new FileInfo(Path.Combine(folder.FullName, bestFileName));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(new(null), package, ErrorType.PackageDownloadFailed));
                return null;
            }
        }
        catch (System.Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageDownloadFailed));
            return null;
        }
    }

    public async Task<bool> InstallAsync(PackageBase package, string id)
    {
        WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(package));

        try
        {
            var installed = await _packageManager.InstallPackageAsync(id);

            if (installed)
            {
                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageInstallCompleted(package));
                return true;
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(new(null), package, ErrorType.PackageInstallFailed));
                return false;
            }
        }
        catch (System.Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageInstallFailed));
            return false;
        }
    }

    private static WinGetPackage CreateSDKPackage(WinGetProxyHandler packageHandler, WGetNET.WinGetPackage cliPackage)
    {
        WinGetPackage sdkPackage = new(packageHandler, cliPackage)
        {
            WinGetId = cliPackage.Id,
            Title = cliPackage.Name,
            Version = cliPackage.AvailableVersionString,
            Status = PackageStatus.BasicDetails
        };

        sdkPackage.Urn = Urn.Parse($"urn:{WinGetProxyHandler.NAMESPACE_WINGET}:{sdkPackage.WinGetId}");

        return sdkPackage;
    }

    private static async Task UpdateSDKPackage(WinGetPackage package)
    {
        Locale locale;
        try
        {
            locale = await CommunityRepo.GetLocaleAsync(package.WinGetId, package.Version, CultureInfo.CurrentUICulture);
        }
        catch (Flurl.Http.FlurlHttpException)
        {
            locale = await CommunityRepo.GetDefaultLocaleAsync(package.WinGetId, package.Version);
        }

        package.Description = locale.Description;
        package.PublisherId = locale.PackageIdentifier.Split('.')[0];
        package.DeveloperName = locale.Publisher ?? package.PublisherId;
        package.Description = locale.Description ?? locale.ShortDescription;
        package.Website = Link.Create(locale.PackageUrl, package.ShortTitle + " website");

        package.Status = PackageStatus.Details;
    }

    private static bool IsNotStoreSourced(WGetNET.WinGetPackage package) => package.SourceName != "msstore";
}

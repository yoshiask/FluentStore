using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.WinGet.Cli;

internal class WinGetCliHandler : IWinGetImplementation
{
    readonly WGetNET.WinGetPackageManager _packageManager = new();

    public async Task<PackageBase> GetPackage(string id, WinGetProxyHandler packageHandler, PackageStatus status = PackageStatus.Details)
    {
        var results = await _packageManager.SearchPackageAsync(id, true);
        var result = results.FirstOrDefault();

        return result is not null
            ? CreateSDKPackage(packageHandler, result)
            : null;
    }

    public async IAsyncEnumerable<PackageBase> SearchAsync(string query, WinGetProxyHandler packageHandler)
    {
        var results = await _packageManager.SearchPackageAsync(query);
        foreach (var result in results)
            yield return CreateSDKPackage(packageHandler, result);
    }

    public async Task<FileSystemInfo> DownloadAsync(WinGetPackage package, DirectoryInfo folder)
    {
        WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(package));

        try
        {
            var downloaded = await _packageManager.DownloadAsync(package.WinGetId, folder);

            if (downloaded)
            {
                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(package));

                // Super-mega-hack because WGetNET doesn't tell us where the file is saved :facepalm:
                var fileNames = folder
                    .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                    .Select(f => f.Name);

                var bestFileName = FuzzySharp.Process.ExtractOne(package.WinGetId, fileNames).Value;
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

    public async Task<bool> InstallAsync(WinGetPackage package)
    {
        WeakReferenceMessenger.Default.Send(new PackageInstallStartedMessage(package));

        try
        {
            var installed = await _packageManager.InstallPackageAsync(package.WinGetId);

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
            Version = cliPackage.AvailableVersion.ToString(),
            Status = PackageStatus.BasicDetails
        };

        sdkPackage.Urn = Urn.Parse($"urn:{WinGetProxyHandler.NAMESPACE_WINGET}:{sdkPackage.WinGetId}");

        return sdkPackage;
    }
}

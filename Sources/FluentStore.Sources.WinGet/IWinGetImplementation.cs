using FluentStore.SDK;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.Sources.WinGet;

internal interface IWinGetImplementation
{
    public Task<PackageBase> GetPackage(string id, WinGetProxyHandler packageHandler, PackageStatus status = PackageStatus.Details);

    public IAsyncEnumerable<PackageBase> SearchAsync(string query, WinGetProxyHandler packageHandler);

    public Task<bool> CanDownloadAsync(PackageBase package, string id);

    public Task<FileSystemInfo> DownloadAsync(PackageBase package, string id, DirectoryInfo folder = null);

    public Task<bool> InstallAsync(PackageBase package, string id);
}

internal static class IWinGetImplementationExtenstions
{
    public static async Task<bool> CanDownloadAsync(this IWinGetImplementation winget, WinGetPackage package)
    {
        return await winget.CanDownloadAsync(package, package.WinGetId);
    }

    public static async Task<FileSystemInfo> DownloadAsync(this IWinGetImplementation winget, WinGetPackage package)
    {
        return await winget.DownloadAsync(package, package.WinGetId);
    }

    public static Task<bool> InstallAsync(this IWinGetImplementation winget, WinGetPackage package)
    {
        return winget.InstallAsync(package, package.WinGetId);
    }
}

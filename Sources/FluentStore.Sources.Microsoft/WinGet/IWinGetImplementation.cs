using FluentStore.SDK;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.Sources.Microsoft.WinGet;

internal interface IWinGetImplementation
{
    public Task<PackageBase> GetPackage(string id, WinGetProxyHandler packageHandler, PackageStatus status = PackageStatus.Details);

    public IAsyncEnumerable<PackageBase> SearchAsync(string query, WinGetProxyHandler packageHandler);

    public Task<bool> CanDownloadAsync(WinGetPackage package);

    public Task<FileSystemInfo> DownloadAsync(WinGetPackage package, DirectoryInfo folder = null);

    public Task<bool> InstallAsync(WinGetPackage package);
}

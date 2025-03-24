using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Scoop;

public interface IScoopPackageManager
{
    Task InstallAsync(string name, CancellationToken token = default);

    Task DownloadAsync(string name, CancellationToken token = default);

    Task UpdateAsync(string name, CancellationToken token = default);

    Task UninstallAsync(string name, CancellationToken token = default);

    IAsyncEnumerable<object> ListInstalledAppsAsync(string name, CancellationToken token = default);

    Task<object> GetAppInfoAsync(string name, CancellationToken token = default);

    Task AddBucketAsync(string name, CancellationToken token = default);
}

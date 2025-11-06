using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Scoop.Responses;

namespace Scoop;

public interface IScoopBucketManager
{
    Task AddBucketAsync(string name, string? repo = null, CancellationToken token = default);

    Task RemoveBucketAsync(string name, CancellationToken token = default);

    IAsyncEnumerable<string> GetKnownBucketsAsync(CancellationToken token = default);

    IAsyncEnumerable<Bucket> GetBucketsAsync(CancellationToken token = default);
}

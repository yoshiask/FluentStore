using Scoop.Responses;
using System.Threading.Tasks;
using System.Threading;
using Flurl;

namespace Scoop;

public interface IScoopMetadataService
{
    Task<Manifest> GetManifestAsync(SearchResultMetadata metadata, CancellationToken token = default);

    Task<Manifest> GetManifestAsync(Url metadataUrl, CancellationToken token = default);
}

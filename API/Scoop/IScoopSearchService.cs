using System.Threading.Tasks;
using System.Threading;
using Scoop.Responses;

namespace Scoop;

public interface IScoopSearchService
{
    Task<SearchResponse> SearchAsync(string query, int count = 20, int skip = 0, CancellationToken token = default);
}

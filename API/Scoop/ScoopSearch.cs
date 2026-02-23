using Flurl.Http;
using Flurl;
using System.Threading.Tasks;
using System.Threading;
using Scoop.Requests;
using Scoop.Responses;

namespace Scoop;

public class ScoopSearch : IScoopSearchService, IScoopMetadataService
{
    private const string SEARCH_URL = "https://scoopsearch.search.windows.net/indexes/apps/docs/search";
    private const string API_VERSION = "2020-06-30";
    private const string PUBLIC_API_KEY = "DC6D2BBE65FC7313F2C52BBD2B0286ED";

    public string UserAgent { get; set; } = "Scoop/.NET";

    public string ApiKey { get; set; } = PUBLIC_API_KEY;

    public async Task<SearchResponse> SearchAsync(string query, int count = 20, int skip = 0, CancellationToken token = default)
    {
        SearchRequest request = new()
        {
            Count = true,
            Top = count,
            Skip = skip,
            Filter = "Metadata/OfficialRepositoryNumber eq 1 and Metadata/DuplicateOf eq null",
            OrderBy = "search.score() desc, Metadata/OfficialRepositoryNumber desc, NameSortable asc",
            Search = query,
            SearchMode = "all",
            Select = "Id,Name,NamePartial,NameSuffix,Description,Notes,Homepage,License,Version,Metadata/Repository,Metadata/FilePath,Metadata/OfficialRepository,Metadata/RepositoryStars,Metadata/Committed,Metadata/Sha"
        };

        return await SearchAsync(request, token);
    }

    public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken token = default)
    {
        var http = await SEARCH_URL
            .WithHeader("User-Agent", UserAgent)
            .WithHeader("api-key", ApiKey)
            .SetQueryParam("api-version", API_VERSION)
            .PostJsonAsync(request, cancellationToken: token);

        return await http.GetJsonAsync<SearchResponse>();
    }

    public Task<Manifest> GetManifestAsync(SearchResultMetadata metadata, CancellationToken token = default)
    {
        return GetManifestAsync(metadata.GetManifestUrl(), token);
    }

    public async Task<Manifest> GetManifestAsync(Url metadataUrl, CancellationToken token = default)
    {
        var manifest = await metadataUrl
            .WithHeader("User-Agent", UserAgent)
            .GetJsonAsync<Manifest>(cancellationToken: token);
        return manifest;
    }
}

using Flurl;
using Flurl.Http;
using ScoopAPI.Requests;
using ScoopAPI.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace ScoopAPI;

public static class ScoopSearch
{
    private const string SEARCH_URL = "https://scoopsearch.search.windows.net/indexes/apps/docs/search";
    private const string API_VERSION = "2020-06-30";

    public static async Task<SearchResponse> SearchAsync(string query, int count = 20, int skip = 0, CancellationToken token = default)
    {
        SearchRequest request = new()
        {
            Count = true,
            Top = count,
            Skip = skip,
            Filter = string.Empty,
            Highlight = "Name,NamePartial,NameSuffix,Description,Version,License,Metadata/Repository,Metadata/AuthorName",
            HighlightPreTag = string.Empty,
            HighlightPostTag = string.Empty,
            OrderBy = "search.score() desc, Metadata/OfficialRepositoryNumber desc, NameSortable asc",
            Search = query,
            SearchMode = "all",
            Select = "Id,Name,NamePartial,NameSuffix,Description,Homepage,License,Version,Metadata/Repository,Metadata/FilePath,Metadata/AuthorName,Metadata/OfficialRepository,Metadata/RepositoryStars,Metadata/Committed,Metadata/Sha"
        };

        var http = await SEARCH_URL.WithHeader("api-key", Secrets.API_KEY)
                .SetQueryParam("api-version", API_VERSION)
                .PostJsonAsync(request, token);

        return await http.GetJsonAsync<SearchResponse>();
    }

    public static Task<Manifest> GetManifestAsync(SearchResultMetadata metadata, CancellationToken token = default)
    {
        return GetManifestAsync(metadata.GetManifestUrl(), token);
    }

    public static async Task<Manifest> GetManifestAsync(Url metadataUrl, CancellationToken token = default)
    {
        var manifest = await metadataUrl.WithHeader("User-Agent", "fluent-store")
            .GetJsonAsync<Manifest>(token);
        return manifest;
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Scoop.Responses;

public class SearchResponse
{
    [JsonPropertyName("@odata.context")]
    public string ODataContext { get; set; }

    [JsonPropertyName("@odata.count")]
    public int ODataCount { get; set; }

    [JsonPropertyName("value")]
    public List<SearchResult> Results { get; set; }
}

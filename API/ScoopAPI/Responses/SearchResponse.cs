using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScoopAPI.Responses;

public class SearchResponse
{
    [JsonProperty("@odata.context")]
    public string ODataContext { get; set; }

    [JsonProperty("@odata.count")]
    public int ODataCount { get; set; }

    [JsonProperty("value")]
    public List<SearchResult> Results { get; set; }
}

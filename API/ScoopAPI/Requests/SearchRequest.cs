using Newtonsoft.Json;

namespace ScoopAPI.Requests;

internal class SearchRequest
{
    [JsonProperty("count")]
    public bool Count { get; set; }

    [JsonProperty("search")]
    public string Search { get; set; }

    [JsonProperty("searchMode")]
    public string SearchMode { get; set; }

    [JsonProperty("filter")]
    public string Filter { get; set; }

    [JsonProperty("orderby")]
    public string OrderBy { get; set; }

    [JsonProperty("skip")]
    public int Skip { get; set; }

    [JsonProperty("top")]
    public int Top { get; set; }

    [JsonProperty("select")]
    public string Select { get; set; }

    [JsonProperty("highlight")]
    public string Highlight { get; set; }

    [JsonProperty("highlightPreTag")]
    public string HighlightPreTag { get; set; }

    [JsonProperty("highlightPostTag")]
    public string HighlightPostTag { get; set; }
}

using System.Text.Json.Serialization;

namespace Scoop.Requests;

internal class SearchRequest
{
    [JsonPropertyName("count")]
    public bool Count { get; set; }

    [JsonPropertyName("search")]
    public string Search { get; set; }

    [JsonPropertyName("searchMode")]
    public string SearchMode { get; set; }

    [JsonPropertyName("filter")]
    public string Filter { get; set; }

    [JsonPropertyName("orderby")]
    public string OrderBy { get; set; }

    [JsonPropertyName("skip")]
    public int Skip { get; set; }

    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("select")]
    public string Select { get; set; }

    [JsonPropertyName("highlight")]
    public string Highlight { get; set; }

    [JsonPropertyName("highlightPreTag")]
    public string HighlightPreTag { get; set; }

    [JsonPropertyName("highlightPostTag")]
    public string HighlightPostTag { get; set; }
}


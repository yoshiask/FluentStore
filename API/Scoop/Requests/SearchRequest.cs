using System.Text.Json.Serialization;

namespace Scoop.Requests;

public class SearchRequest
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
    public int Top { get; set; } = 20;

    [JsonPropertyName("select")]
    public string Select { get; set; }

    [JsonPropertyName("highlight")]
    public string Highlight { get; set; } = string.Empty;

    [JsonPropertyName("highlightPreTag")]
    public string HighlightPreTag { get; set; } = string.Empty;

    [JsonPropertyName("highlightPostTag")]
    public string HighlightPostTag { get; set; } = string.Empty;
}


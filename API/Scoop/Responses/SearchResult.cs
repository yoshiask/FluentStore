using System.Text.Json.Serialization;

namespace Scoop.Responses;

public class SearchResult
{
    [JsonPropertyName("@search.score")]
    public double SearchScore { get; set; }

    public string Id { get; set; }
    public string Name { get; set; }
    public string NamePartial { get; set; }
    public string NameSuffix { get; set; }
    public string Description { get; set; }
    public string Homepage { get; set; }
    public string License { get; set; }
    public string Version { get; set; }
    public SearchResultMetadata Metadata { get; set; }
}

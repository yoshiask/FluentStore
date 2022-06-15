using Newtonsoft.Json;

namespace Winstall.Models;

internal sealed class AppPageResponse
{
    [JsonProperty("props")]
    public Response<IndexPageProps> Props { get; set; }

    public string Page { get; set; }

    public object Query { get; set; }

    public string BuildId { get; set; }

    public bool IsFallback { get; set; }

    public bool GSP { get; set; }
}

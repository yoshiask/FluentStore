using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V2
{
    public class SupportUri
    {
        [JsonProperty(PropertyName = "Uri")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri
            => Uri.TryCreate(Url, UriKind.Absolute, out var uri)
            ? uri : null;
    }
}

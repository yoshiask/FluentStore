using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Marketplace.Storefront.Contracts.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TileLayout
    {
        Portrait, 
        Poster, 
        Square,
        Circular,
    };
}

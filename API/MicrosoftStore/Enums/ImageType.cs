using Newtonsoft.Json;

namespace Microsoft.Marketplace.Storefront.Contracts.Enums
{
    [JsonConverter(typeof(ImageTypeStringConverter))]
    public enum ImageType
    {
        Unspecified,
        BoxArt,
        Logo,
        Poster,
        Tile,
        Hero,
        Screenshot,
        Trailer
    };
}

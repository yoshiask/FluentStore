using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MicrosoftStore.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ImageType
    {
        BoxArt,
        Logo,
        Poster,
        Tile,
        Hero,
        Screenshot,
        Trailer
    };
}

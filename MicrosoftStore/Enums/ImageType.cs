using Newtonsoft.Json;

namespace MicrosoftStore.Enums
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

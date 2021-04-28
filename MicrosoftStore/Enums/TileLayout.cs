using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MicrosoftStore.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TileLayout
    {
        Portrait, 
        Poster, 
        Square
    };
}

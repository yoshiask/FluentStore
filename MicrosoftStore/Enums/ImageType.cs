using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Screenshot
    };
}

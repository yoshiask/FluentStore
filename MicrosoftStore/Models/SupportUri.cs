using Newtonsoft.Json;
using System;

namespace MicrosoftStore.Models
{
    public class SupportUri
    {
        [JsonProperty(PropertyName = "Uri")]
        public string Url { get; set; }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri(Url);
                }
                catch { return null; }
            }
        }
    }
}

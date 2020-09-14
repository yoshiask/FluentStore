using Newtonsoft.Json;
using System;

namespace MicrosoftStore.Responses
{
    public class ResponseItem
    {
        [JsonProperty(PropertyName = "$type")]
        public string TypeName { get; set; }
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public object Payload { get; set; }
    }
}

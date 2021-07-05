using Newtonsoft.Json;
using System;

namespace MicrosoftStore.Responses
{
    public class ResponseItem
    {
        [JsonProperty(PropertyName = "$type", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName { get; set; }
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public object Payload { get; set; }

        public ResponseItem<TPayload> Convert<TPayload>() where TPayload : Payload
        {
            return new ResponseItem<TPayload>(this);
        }
    }

    public class ResponseItem<TPayload> where TPayload : Payload
    {
        [JsonProperty(PropertyName = "$type", NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName { get; set; }
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public TPayload Payload { get; set; }

        public ResponseItem(ResponseItem item)
        {
            if (item != null)
            {
                TypeName = item.TypeName;
                Path = item.Path;
                ExpiryUtc = item.ExpiryUtc;
                Payload = (item.Payload as Newtonsoft.Json.Linq.JObject).ToObject<TPayload>();
            }
        }
    }
}

using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V1
{
    public class ResponseItem
    {
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public object Payload { get; set; }

        public ResponseItem<TPayload> Convert<TPayload>()
        {
            return new ResponseItem<TPayload>(this);
        }
    }

    public class ResponseItem<TPayload>
    {
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public TPayload Payload { get; set; }

        public ResponseItem(ResponseItem item)
        {
            if (item != null)
            {
                Path = item.Path;
                ExpiryUtc = item.ExpiryUtc;
                Payload = (item.Payload as JObject).ToObject<TPayload>();
            }
        }
    }
}

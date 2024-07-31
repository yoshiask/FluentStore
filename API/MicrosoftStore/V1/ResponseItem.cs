using Newtonsoft.Json.Linq;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V1
{
    public interface IResponseItem<out TPayload>
    {
        DateTimeOffset ExpiryUtc { get; }
        string Path { get; }
        TPayload Payload { get; }
    }

    public class ResponseItem : IResponseItem<object>
    {
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public object Payload { get; set; }

        public ResponseItem<TPayload> Convert<TPayload>()
        {
            return new ResponseItem<TPayload>(this);
        }
    }

    public class ResponseItem<TPayload> : IResponseItem<TPayload>
    {
        public string Path { get; }
        public DateTimeOffset ExpiryUtc { get; }
        public TPayload Payload { get; }

        public ResponseItem(ResponseItem item)
        {
            if (item != null)
            {
                Path = item.Path;
                ExpiryUtc = item.ExpiryUtc;
                Payload = (item.Payload is JObject json)
                    ? json.ToObject<TPayload>()
                    : (TPayload)item.Payload;
            }
        }
    }
}

using Microsoft.Marketplace.Storefront.Contracts.V1;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    public class ResponseItemList : List<ResponseItem>
    {
        public List<T> GetPayloads<T>() where T : class
        {
            return FindAll(i => i.Payload.GetType() == typeof(T)).ConvertAll(i => (T)i.Payload);
        }

        public T GetPayload<T>() where T : class => GetPayloads<T>().FirstOrDefault();

        public bool TryGetPayloads<T>(out List<T> payloads) where T : class
        {
            payloads = GetPayloads<T>();
            return payloads != null && payloads.Count > 0;
        }

        public bool TryGetPayload<T>(out T payload) where T : class
        {
            payload = GetPayload<T>();
            return payload != null;
        }
    }
}

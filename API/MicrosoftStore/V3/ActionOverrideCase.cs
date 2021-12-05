using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ActionOverrideCase
    {
        public ActionOverrideConditions Conditions { get; set; }
        public bool Visibility { get; set; }
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

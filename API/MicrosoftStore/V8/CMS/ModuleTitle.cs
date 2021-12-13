using Newtonsoft.Json;

namespace Microsoft.Marketplace.Storefront.Contracts.V8.CMS
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
    public class ModuleTitle
    {
        public string Text { get; set; }
        public string Id { get; set; }
    }
}

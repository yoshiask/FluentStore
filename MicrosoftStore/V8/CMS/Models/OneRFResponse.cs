using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V8.CMS.Models
{
    public class OneRFResponse
    {
        public PageObject PageObject { get; set; }
        [JsonIgnore]    // TODO: Json.NET fails to find a class matching "Cms.moduleTitle"
        public List<KeyValuePair<List<string>, List<ModuleTitle>>> CmsData { get; set; }
    }
}

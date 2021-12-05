using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V8
{
    public class Module
    {
        public string Id { get; set; }
        public string ModuleDefinitionName { get; set; }
        public bool IsCritical { get; set; }
        public List<DataSource> DataSources { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public List<Module> Modules { get; set; }
    }
}

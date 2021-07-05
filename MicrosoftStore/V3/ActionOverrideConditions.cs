using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ActionOverrideConditions
    {
        public List<string> ClassicAppKeys { get; set; }
        public PlatformCondition Platform { get; set; }
    }
}

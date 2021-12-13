using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ActionOverride
    {
        public string ActionType { get; set; }
        public List<ActionOverrideCase> Cases { get; set; }
    }
}

using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class PackageRequirements
    {
        public List<string> HardwareRequirements { get; set; }
        public List<string> SupportedArchitectures { get; set; }
        public List<Platform> PlatformDependencies { get; set; }
    }
}

using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class PackageRequirements
    {
        public List<string> HardwareRequirements { get; set; }
        public List<string> SupportedArchitectures { get; set; }
        public List<Platform> PlatformDependencies { get; set; }
    }

    public class Platform
    {
        public string PlatformName { get; set; }
        public long MaxTested { get; set; }
        public long MinVersion { get; set; }
    }
}

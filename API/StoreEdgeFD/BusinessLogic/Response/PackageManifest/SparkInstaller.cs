using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class SparkInstaller
    {
        public string InstallerSha256 { get; set; }
        public string InstallerUrl { get; set; }
        public string InstallerLocale { get; set; }
        public string MinimumOSVersion { get; set; }
        public Switches? InstallerSwitches { get; set; }
        public WinGetRun.Enums.InstallerArchitecture Architecture { get; set; }
        public WinGetRun.Enums.InstallerType InstallerType { get; set; }
        public Markets Markets { get; set; }

        [JsonIgnore]
        public Uri? InstallerUri
        {
            get
            {
                try
                {
                    return new(InstallerUrl);
                }
                catch
                {
                    return null;
                }
            }
        }

        public WinGetRun.Models.Installer ToWinGetRunInstaller()
        {
            return new()
            {
                Arch = Architecture,
                InstallerType = InstallerType,
                Language = InstallerLocale,
                Sha256 = InstallerSha256,
                SignatureSha256 = InstallerSha256,
                Switches = InstallerSwitches,
                Url = InstallerUrl,
            };
        }
    }

    public class Switches : WinGetRun.Models.Switches { }
}
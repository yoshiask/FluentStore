using Newtonsoft.Json;
using System;
using Winstall.Enums;
using Winstall.Models.Manifest;
using Winstall.Models.Manifest.Enums;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class SparkInstaller
    {
        public string InstallerSha256 { get; set; }
        public string InstallerUrl { get; set; }
        public string InstallerLocale { get; set; }
        public string MinimumOSVersion { get; set; }
        public InstallerSwitches? InstallerSwitches { get; set; }
        public InstallerArchitecture Architecture { get; set; }
        public InstallerType InstallerType { get; set; }
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

        public Installer ToWinstaller()
        {
            return new()
            {
                Architecture = Architecture,
                InstallerType = InstallerType,
                InstallerLocale = InstallerLocale,
                InstallerSha256 = InstallerSha256,
                SignatureSha256 = InstallerSha256,
                InstallerSwitches = InstallerSwitches,
                InstallerUrl = InstallerUrl,
            };
        }
    }
}
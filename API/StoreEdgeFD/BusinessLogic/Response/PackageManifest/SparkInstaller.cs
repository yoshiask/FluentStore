using Newtonsoft.Json;
using System;
using WinGet.Sharp.Enums;
using WinGet.Sharp.Models;

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

        public Installer ToWinGet()
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
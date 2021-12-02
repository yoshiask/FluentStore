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
    }

    public class Switches : WinGetRun.Models.Switches { }
}
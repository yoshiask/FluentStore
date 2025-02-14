using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class PackageManifestVersion
    {
        public string PackageVersion { get; set; }
        public DefaultLocale DefaultLocale { get; set; }
        public List<DefaultLocale> Locales { get; set; }
        public List<SparkInstaller> Installers { get; set; }
    }
}
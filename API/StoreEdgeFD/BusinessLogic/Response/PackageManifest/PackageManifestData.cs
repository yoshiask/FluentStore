using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class PackageManifestData
    {
        public string PackageIdentifier { get; set; }
        public List<PackageManifestVersion> Versions { get; set; }
    }
}
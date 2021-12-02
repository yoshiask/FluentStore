using Flurl.Http;
using System.Globalization;
using System.Threading.Tasks;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic
{
    public class StoreEdgeFDApi
    {
        /// <summary>
        /// Gets the package manifest for the given package.
        /// </summary>
        public async Task<Response.PackageManifest.PackageManifestResponse> GetPackageManifest(string packageId, CultureInfo culture = null)
        {
            return await UrlEx.GetStoreEdgeFDBase(culture).AppendPathSegments("packageManifests", packageId)
                .GetJsonAsync<Response.PackageManifest.PackageManifestResponse>();
        }
    }
}

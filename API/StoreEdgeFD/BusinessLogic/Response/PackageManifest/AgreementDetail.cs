using Newtonsoft.Json;
using System;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class AgreementDetail
    {
        public string AgreementLabel { get; set; }
        public string? Agreement { get; set; }
        public string? AgreementUrl { get; set; }

        [JsonIgnore]
        public Uri? AgreementUri
        {
            get
            {
                if (AgreementUrl != null)
                    return new(AgreementUrl);
                return null;
            }
        }
    }
}
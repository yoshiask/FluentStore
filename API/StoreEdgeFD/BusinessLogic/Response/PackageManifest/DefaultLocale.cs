using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest
{
    public class DefaultLocale
    {
        public string PackageLocale { get; set; }
        public string Publisher { get; set; }
        public string PublisherUrl { get; set; }
        public string PrivacyUrl { get; set; }
        public string PublisherSupportUrl { get; set; }
        public string PackageName { get; set; }
        public string License { get; set; }
        public string Copyright { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<AgreementDetail> Agreements { get; set; }

        [JsonIgnore]
        public Uri? PublisherUri
        {
            get
            {
                if (PublisherUrl != null)
                    return new(PublisherUrl);
                return null;
            }
        }

        [JsonIgnore]
        public Uri? PrivacyUri
        {
            get
            {
                if (PrivacyUrl != null)
                    return new(PrivacyUrl);
                return null;
            }
        }

        [JsonIgnore]
        public Uri? PublisherSupportUri
        {
            get
            {
                if (PublisherSupportUrl != null)
                    return new(PublisherSupportUrl);
                return null;
            }
        }

        [JsonIgnore]
        public Uri? LicenseUri
        {
            get
            {
                if (License != null)
                    return new(License);
                return null;
            }
        }

        [JsonIgnore]
        public Uri? CopyrightUri
        {
            get
            {
                try
                {
                    return new(Copyright);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
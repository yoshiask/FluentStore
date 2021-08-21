using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluentStore.SDK.Handlers
{
    public class MicrosoftStoreHandler : PackageHandlerBase
    {
        private readonly StorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<StorefrontApi>();

        public const string NAMESPACE_MSSTORE = "microsoft-store";
        public const string NAMESPACE_MODERNPACK = "win-modern-package";
        public override HashSet<string> HandledNamespaces => new HashSet<string>
        {
            NAMESPACE_MSSTORE,
            NAMESPACE_MODERNPACK,
        };

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            var firstPage = await StorefrontApi.Search(query, "apps", Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily);
            foreach (var product in firstPage.Payload.SearchResults)
            {
                var package = new MicrosoftStorePackage(Image, product);
                package.Status = PackageStatus.BasicDetails;
                packages.Add(package);
            }

            return packages;
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var suggs = await StorefrontApi.GetSearchSuggestions(query);
            var packages = new List<PackageBase>();

            foreach (var product in suggs.Payload.AssetSuggestions)
            {
                var package = new MicrosoftStorePackage(Image, summary: product);
                package.Status = PackageStatus.BasicDetails;
                packages.Add(package);
            }

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_MSSTORE, nameof(packageUrn));

            string productId = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            return await GetPackageFromPage(productId);
        }

        private async Task<MicrosoftStorePackage> GetPackageFromPage(string productId)
        {
            var page = await StorefrontApi.GetPage(productId);

            if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
            {
                // The Storefront API doesn't return 404 if the request was valid
                // but no such produdct exists, so it has to be caught manually.
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V1.ErrorResponse>(out var error))
                {
                    var NavService = Ioc.Default.GetRequiredService<Services.INavigationService>();
                    uint code = uint.Parse(error.ErrorCode, System.Globalization.NumberStyles.AllowHexSpecifier);
                    NavService.ShowHttpErrorPage(404, error.ErrorDescription);
                }
                return null;
            }

            var package = new MicrosoftStorePackage(Image, product: details);
            if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.RatingSummary>(out var ratingSummary))
            {
                package.Update(ratingSummary);
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ReviewList>(out var reviewList))
                {
                    package.Update(reviewList);
                }
            }

            package.Status = PackageStatus.Details;
            return package;
        }
        
        public override ImageBase GetImage()
        {
            return new FileImage
            {
                Url = "ms-appx:///Assets/PackageHandlerIcons/MicrosoftStoreHandler/StoreAppList.png"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            Regex rx = new Regex(@"^https?:\/\/(?:www\.)?microsoft\.com\/(?:store\/apps|(?<locale>[a-z]{2}-[a-z]{2})\/p\/.+)\/(?<id>\w{12})",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = rx.Match(url.ToString());
            if (!m.Success)
                return null;

            return await GetPackage(Urn.Parse($"urn:{NAMESPACE_MSSTORE}:{m.Groups["id"]}"));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (!(package is MicrosoftStorePackage msPackage))
                throw new System.ArgumentException();
            return "https://www.microsoft.com/store/apps/" + msPackage.StoreId;
        }
    }
}

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
                // Get the full product details
                var page = await StorefrontApi.GetPage(product.ProductId);
                if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
                    continue;

                var package = new MicrosoftStorePackage(Image, details);
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.RatingSummary>(out var ratingSummary))
                {
                    package.Update(ratingSummary);
                    if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ReviewList>(out var reviewList))
                    {
                        package.Update(reviewList);
                    }
                }

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
                // Get the full product details
                var page = await StorefrontApi.GetPage(product.ProductId);
                if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
                    continue;

                var package = new MicrosoftStorePackage(Image, details);
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.RatingSummary>(out var ratingSummary))
                {
                    package.Update(ratingSummary);
                    if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ReviewList>(out var reviewList))
                    {
                        package.Update(reviewList);
                    }
                }

                packages.Add(package);
            }

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_MSSTORE, nameof(packageUrn));

            string productId = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            var page = await StorefrontApi.GetPage(productId);
            if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
                return null;

            var package = new MicrosoftStorePackage(Image, details);
            if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.RatingSummary>(out var ratingSummary))
            {
                package.Update(ratingSummary);
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ReviewList>(out var reviewList))
                {
                    package.Update(reviewList);
                }
            }

            return package;
        }
        
        public override ImageBase GetImage()
        {
            return new TextImage
            {
                Text = "\uE14D",
                FontFamily = "Segoe MDL2 Assets"
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

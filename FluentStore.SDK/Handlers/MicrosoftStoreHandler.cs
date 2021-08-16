using FluentStore.SDK.Packages;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
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
            var firstPage = await StorefrontApi.Search(query, "apps", "Windows.Desktop");
            foreach (var product in firstPage.Payload.SearchResults)
            {
                // Get the full product details
                var page = await StorefrontApi.GetPage(product.ProductId);
                if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
                    continue;

                var package = new MicrosoftStorePackage(details);
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

                var package = new MicrosoftStorePackage(details);
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

            var package = new MicrosoftStorePackage(details);
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
    }
}

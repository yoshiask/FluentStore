using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK.Handlers
{
    public class MicrosoftStoreHandler : PackageHandlerBase
    {
        private readonly StorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<StorefrontApi>();

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            var firstPage = await StorefrontApi.Search(query, "apps", "Windows.Desktop");
            foreach (var product in firstPage.Payload.SearchResults)
            {
                // Get the full product details
                var item = await StorefrontApi.GetProduct(product.ProductId);
                var candidate = item.Payload;
                if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                {
                    packages.Add(new MicrosoftStorePackage(candidate));
                }
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
                var item = await StorefrontApi.GetProduct(product.ProductId);
                var candidate = item.Payload;
                if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                {
                    packages.Add(new MicrosoftStorePackage(candidate));
                }
            }

            return packages;
        }
    }
}

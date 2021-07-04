using FluentStore.SDK.Packages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using MicrosoftStore;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace FluentStore.SDK.Handlers
{
    public class MicrosoftStoreHandler : PackageHandlerBase
    {
        private readonly IMSStoreApi MSStoreApi = Ioc.Default.GetRequiredService<IMSStoreApi>();
        private readonly IStorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<IStorefrontApi>();

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            RegionInfo region = new RegionInfo(culture.LCID);
            int pageSize = 25;

            var packages = new List<PackageBase>();
            var firstPage = await StorefrontApi.Search(query, region.TwoLetterISORegionName, culture.Name, "apps", "all", "Windows.Desktop", pageSize, 0);
            foreach (var product in firstPage.Payload.Cards)
            {
                // Get the full product details
                var item = await StorefrontApi.GetProduct(product.ProductId, region.TwoLetterISORegionName, culture.Name);
                var candidate = item.Convert<ProductDetails>().Payload;
                if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                {
                    packages.Add(new MicrosoftStorePackage(candidate));
                }
            }

            double requestCount = Math.Ceiling((double)firstPage.Payload.TotalItems / pageSize);
            for (int i = 1; i < requestCount; i++)
            {
                var search = await StorefrontApi.Search(
                    query, region.TwoLetterISORegionName, culture.Name,
                    "apps", "all", "Windows.Desktop",
                    pageSize, i * pageSize);
                foreach (var product in search.Payload.Cards)
                {
                    packages.Add(new MicrosoftStorePackage(product));
                }
            }

            return packages;
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var culture = CultureInfo.CurrentUICulture;
            var region = new RegionInfo(culture.LCID);

            var suggs = await MSStoreApi.GetSuggestions(
                query, culture.Name, Constants.CLIENT_ID,
                new string[] { Constants.CAT_ALL_PRODUCTS }, new int[] { 10, 0, 0 }
            );
            var packages = new List<PackageBase>();
            if (suggs.ResultSets.Count <= 0)
                return packages;
            
            foreach (var product in suggs.ResultSets[0].Suggests.Where(s => s.Source == "Apps"))
            {
                string productId = product.Metas.First(m => m.Key == "BigCatalogId").Value;

                // Get the full product details
                var item = await StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name);
                var candidate = item.Convert<ProductDetails>().Payload;
                if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                {
                    packages.Add(new MicrosoftStorePackage(candidate));
                }
            }

            return packages;
        }
    }
}

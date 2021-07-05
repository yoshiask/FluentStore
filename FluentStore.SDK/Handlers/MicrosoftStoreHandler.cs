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
        private readonly StorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<StorefrontApi>();

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            int pageSize = 25;

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

            //double requestCount = Math.Ceiling((double)firstPage.Payload.TotalItems / pageSize);
            //for (int i = 1; i < requestCount; i++)
            //{
            //    var search = await StorefrontApi.Search(
            //        query, region.TwoLetterISORegionName, culture.Name,
            //        "apps", "all", "Windows.Desktop",
            //        pageSize, i * pageSize);
            //    foreach (var product in search.Payload.Cards)
            //    {
            //        packages.Add(new MicrosoftStorePackage(product));
            //    }
            //}

            return packages;
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var suggs = await StorefrontApi.GetSearchSuggestions(query);
            var packages = new List<PackageBase>();

            //var culture = CultureInfo.CurrentUICulture;
            //var region = new RegionInfo(culture.LCID);

            //var suggs = await MSStoreApi.GetSuggestions(
            //    query, culture.Name, Constants.CLIENT_ID,
            //    new string[] { Constants.CAT_ALL_PRODUCTS }, new int[] { 10, 0, 0 }
            //);
            //if (suggs.ResultSets.Count <= 0)
            //    return packages;

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

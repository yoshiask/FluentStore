using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using StoreLib.Models;
using StoreLib.Services;

namespace StoreLib.Cli
{
    public static class CommandHandler
    {
        public static string Token = null;

        private static readonly MSHttpClient _httpClient = new MSHttpClient();

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private static async Task<string> GetPrintablePackageLink(Uri uri, string entryName = "UNKNOWN")
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = uri;
            httpRequest.Method = HttpMethod.Head;
            httpRequest.Headers.Add("Connection", "Keep-Alive");
            httpRequest.Headers.Add("Accept", "*/*");
            httpRequest.Headers.Add("User-Agent", "Microsoft-Delivery-Optimization/10.0");
            HttpResponseMessage httpResponse = await _httpClient.SendAsync(httpRequest, new System.Threading.CancellationToken());
            HttpHeaders headers = httpResponse.Content.Headers;
            IEnumerable<string> values;
            string packagelink;
            if (headers.TryGetValues("Content-Disposition", out values))
            {
                ContentDisposition contentDisposition = new ContentDisposition(values.First());
                string filename = contentDisposition.FileName;
                packagelink = $"[{filename}]({uri})";
            }
            else
            {
                //temporarily hold the value of the new package in a seperate var in order to check if the field will be too long
                packagelink = $"[{entryName}]({uri})";
            }
            if (headers.TryGetValues("Content-Length", out values))
            {
                string filesize = BytesToString(long.Parse(values.FirstOrDefault()));
                packagelink += $": {filesize}";
            }
            return packagelink;
        }

        /// <summary>
        /// Get package download URLs
        /// </summary>
        /// <param name="dcatHandler">Instance of DisplayCatalogHandler</param>
        /// <param name="productId">Product Id (e.g. 9wzdncrfj3tj)</param>
        /// <returns></returns>
        public static async Task PackagesAsync(DisplayCatalogHandler dcatHandler, string id, IdentiferType type)
        {
            if (String.IsNullOrEmpty(Token))
                await dcatHandler.QueryDCATAsync(id, type);
            else
                await dcatHandler.QueryDCATAsync(id, type, Token);
            
            if (!dcatHandler.IsFound)
            {
                Console.WriteLine("Product not found!");
                return;
            }

            if (dcatHandler.ProductListing.Product != null) //One day ill fix the mess that is the StoreLib JSON, one day.
            {
                dcatHandler.ProductListing.Products = new List<Product>();
                dcatHandler.ProductListing.Products.Add(dcatHandler.ProductListing.Product);
            }

            var product = dcatHandler.ProductListing.Products[0];
            var props = product.DisplaySkuAvailabilities[0].Sku.Properties;
            var header = $"{product.LocalizedProperties[0].ProductTitle} - {product.LocalizedProperties[0].PublisherName}";


            Console.WriteLine($"Processing {header}");

            if (props.FulfillmentData == null)
            {
                Console.WriteLine("FullfilmentData empty");
                return;
            }

            var packages = await dcatHandler.GetPackagesForProductAsync();
            //iterate through all packages
            foreach (PackageInstance package in packages)
            {
                var line = await GetPrintablePackageLink(package.PackageUri, package.PackageMoniker);
                Console.WriteLine(line);
            }

            if (props.Packages.Count == 0 ||
                props.Packages[0].PackageDownloadUris == null)
            {
                Console.WriteLine("Packages.Count == 0");
            }

            foreach (var Package in props.Packages[0].PackageDownloadUris)
            {
                var line = await GetPrintablePackageLink(new Uri(Package.Uri));
                Console.WriteLine(line);
            }
        }
        
        /// <summary>
        /// Query for detailed product information
        /// </summary>
        /// <param name="dcatHandler"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task AdvancedQueryAsync(DisplayCatalogHandler dcatHandler, string id, IdentiferType type)
        {
            if (String.IsNullOrEmpty(Token))
                await dcatHandler.QueryDCATAsync(id, type);
            else
                await dcatHandler.QueryDCATAsync(id, type, Token);

            if (!dcatHandler.IsFound)
            {
                Console.WriteLine("Product not found");
                return;
            }

            if (dcatHandler.ProductListing.Product != null) //One day ill fix the mess that is the StoreLib JSON, one day.
            {
                dcatHandler.ProductListing.Products = new List<Product>();
                dcatHandler.ProductListing.Products.Add(dcatHandler.ProductListing.Product);
            }

            var product = dcatHandler.ProductListing.Product;

            Console.WriteLine("App Info:");
            Console.WriteLine($"{product.LocalizedProperties[0].ProductTitle} - {product.LocalizedProperties[0].PublisherName}");
            Console.WriteLine($"Description: {product.LocalizedProperties[0].ProductDescription}");
            Console.WriteLine($"Rating: {product.MarketProperties[0].UsageData[0].AverageRating} Stars");
            Console.WriteLine($"Last Modified: {product.MarketProperties[0].OriginalReleaseDate.ToString()}");
            Console.WriteLine($"Product Type: {product.ProductType}");
            Console.WriteLine($"Is a Microsoft Listing: {product.IsMicrosoftProduct.ToString()}");
            if (product.ValidationData != null)
            {
                Console.WriteLine($"Validation Info: `{product.ValidationData.RevisionId}`");
            }
            if (product.SandboxID != null)
            {
                Console.WriteLine($"SandBoxID: {product.SandboxID}");
            }

            //Dynamicly add any other ID(s) that might be present rather than doing a ton of null checks.
            foreach (AlternateId PID in product.AlternateIds)
            {
                Console.WriteLine($"{PID.IdType}: {PID.Value}");
            }

            var skuProps = product.DisplaySkuAvailabilities[0].Sku.Properties;
            if (skuProps.FulfillmentData != null)
            {
                if (skuProps.Packages[0].KeyId != null)
                {
                    Console.WriteLine($"EAppx Key ID: {skuProps.Packages[0].KeyId}");
                }
                Console.WriteLine($"WuCategoryID: {skuProps.FulfillmentData.WuCategoryId}");
            }
        }

        /// <summary>
        /// Enumerate content via search query
        /// </summary>
        /// <param name="dcatHandler"></param>
        /// <param name="query"></param>
        /// <param name="deviceFamily"></param>
        /// <returns></returns>
        public static async Task SearchAsync(DisplayCatalogHandler dcatHandler, string query, DeviceFamily deviceFamily)
        {
            try
            {
                DCatSearch results = await dcatHandler.SearchDCATAsync(query, deviceFamily);

                Console.WriteLine($"Search results (Count: {results.TotalResultCount}, Family: {deviceFamily})");

                foreach (Result res in results.Results)
                {
                    foreach (Product prod in res.Products)
                    {
                        Console.WriteLine($"{prod.Title} {prod.Type}: {prod.ProductId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while executing SearchAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Convert the provided id to other formats
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static async Task ConvertId(DisplayCatalogHandler dcatHandler, string id, IdentiferType type)
        {
            if (String.IsNullOrEmpty(Token))
                await dcatHandler.QueryDCATAsync(id, type);
            else
                await dcatHandler.QueryDCATAsync(id, type, Token);

            if (!dcatHandler.IsFound)
            {
                Console.WriteLine("No Product found!");
                return;
            }

            if (dcatHandler.ProductListing.Product != null) //One day ill fix the mess that is the StoreLib JSON, one day. Yeah mate just like how one day i'll learn how to fly
            {
                dcatHandler.ProductListing.Products = new List<Product>();
                dcatHandler.ProductListing.Products.Add(dcatHandler.ProductListing.Product);
            }

            var product = dcatHandler.ProductListing.Products[0];

            Console.WriteLine("App info:");
            Console.WriteLine($"{product.LocalizedProperties[0].ProductTitle} - {product.LocalizedProperties[0].PublisherName}");

            //Dynamicly add any other ID(s) that might be present rather than doing a ton of null checks.
            foreach (AlternateId PID in product.AlternateIds)
            {
                Console.WriteLine($"{PID.IdType}: {PID.Value}");
            }

            //Add the product ID
            Console.WriteLine($"ProductID: {product.ProductId}");
            
            try
            {
                //Add the package family name
                Console.WriteLine($"PackageFamilyName: {product.Properties.PackageFamilyName}");
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to add PFN: {ex}");
            };
        }
    }
}
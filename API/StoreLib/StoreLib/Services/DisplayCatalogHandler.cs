
using StoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StoreLib.Services
{
    public class DisplayCatalogHandler
    {
        private readonly MSHttpClient _httpClient;

        public DisplayCatalogModel ProductListing { get; internal set; }
        public Exception Error { get; internal set; } 
        internal Uri ConstructedUri { get; set; }
        public DCatEndpoint SelectedEndpoint; 
        public DisplayCatalogResult Result { get; internal set; }
        public DeviceFamily DeviceFamily; 
        public DCatSearch SearchResult { get; internal set; }
        public string ID;
        public Locale SelectedLocale;
        public bool IsFound = false;


        public DisplayCatalogHandler(DCatEndpoint SelectedEndpoint, Locale Locale)
        {
            //Adds needed headers for MS related requests. See MS_httpClient.cs
            this._httpClient = new MSHttpClient();

            this.SelectedEndpoint = SelectedEndpoint;
            this.SelectedLocale = Locale;
        }

        public static DisplayCatalogHandler ProductionConfig()
        {
            return new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(Market.US, Lang.en, true));
        }


        /// <summary>
        /// Returns an IList of Uris containing the direct download links for the product's apps and dependacies. (if it has any). 
        /// </summary>
        /// <returns>IList of Direct File URLs</returns>
        public async Task<IList<PackageInstance>> GetPackagesForProductAsync()
        {
            string xml = await FE3Handler.SyncUpdatesAsync(ProductListing.Product.DisplaySkuAvailabilities[0].Sku.Properties.FulfillmentData.WuCategoryId);
            IList<string> RevisionIDs;
            IList<string> PackageNames;
            IList<string> UpdateIDs;
            FE3Handler.ProcessUpdateIDs(xml, out RevisionIDs, out PackageNames, out UpdateIDs);
            IList<PackageInstance> PackageInstances = await FE3Handler.GetPackageInstancesAsync(ProductListing.Product.DisplaySkuAvailabilities[0].Sku.Properties.FulfillmentData.WuCategoryId);
            var Files = await FE3Handler.GetFileUrlsAsync(UpdateIDs, RevisionIDs);
            foreach(PackageInstance package in PackageInstances)
            {
                package.PackageUri = Files.Where(x => x.digest == package.Digest).FirstOrDefault().url;
            }
            return PackageInstances;
        }

        public async Task<IList<PackageInstance>> GetMainPackagesForProductAsync()
        {
            string xml = await FE3Handler.SyncUpdatesAsync(ProductListing.Product.DisplaySkuAvailabilities[0].Sku.Properties.FulfillmentData.WuCategoryId);
            IList<string> RevisionIDs;
            IList<string> PackageNames;
            IList<string> UpdateIDs;
            FE3Handler.ProcessMainPackageUpdateIDs(xml, out RevisionIDs, out PackageNames, out UpdateIDs);

            string packageFamilyName = ProductListing.Product.Properties.PackageFamilyName;
            IList<PackageInstance> PackageInstances = await FE3Handler.GetPackageInstancesAsync(ProductListing.Product.DisplaySkuAvailabilities[0].Sku.Properties.FulfillmentData.WuCategoryId);
            PackageInstances = PackageInstances.ToList().FindAll(p => {
                return p.PackageType != PackageType.Unknown
                    && (p.PackageFamily != null) && ($"{p.PackageFamily}_{p.PublisherId}" == packageFamilyName);
            });
            var Files = await FE3Handler.GetFileUrlsAsync(UpdateIDs, RevisionIDs);
            foreach (PackageInstance package in PackageInstances)
            {
                package.PackageUri = Files.Where(x => x.digest == package.Digest).FirstOrDefault().url;
            }
            return PackageInstances;
        }

        /// <summary>
        /// Queries DisplayCatalog for the provided ID. The resulting possibly found product is reflected in DisplayCatalogHandlerInstance.ProductListing. If the product isn't found, that variable will be null, check IsFound and Result.
        /// The provided Auth Token is also sent allowing for flighted or sandboxed listings. The resulting possibly found product is reflected in DisplayCatalogHandlerInstance.ProductListing. If the product isn't found, that variable will be null, check IsFound and Res
        /// </summary>
        /// <param name="ID">The ID, type specified in DCatHandler Instance.</param>
        /// <param name="IDType">Type of ID being passed.</param>
        /// <param name="AuthenticationToken"></param>
        /// <returns></returns>
        public async Task QueryDCATAsync(string ID, IdentiferType IDType = IdentiferType.ProductID, string AuthenticationToken = null) //Optional Authentication Token used for Sandbox and Flighting Queries.
        {
            this.ID = ID;
            this.ConstructedUri = Utilities.UriHelpers.CreateAlternateDCatUri(SelectedEndpoint, ID, IDType, SelectedLocale);
            Result = new DisplayCatalogResult(); //We need to clear the result incase someone queries a product, then queries a not found one, the wrong product will be returned.
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            HttpRequestMessage httpRequestMessage;
            //We need to build the request URL based on the requested EndPoint;
            httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, ConstructedUri);

            if (!String.IsNullOrEmpty(AuthenticationToken))
            {
                httpRequestMessage.Headers.TryAddWithoutValidation("Authentication", AuthenticationToken);
            }

            try
            {
                httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
            }
            catch (TaskCanceledException)
            {
                Result = DisplayCatalogResult.TimedOut;
            }
            if (httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync();
                Result = DisplayCatalogResult.Found;
                IsFound = true;
                ProductListing = DisplayCatalogModel.FromJson(content);
            }
            else if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Result = DisplayCatalogResult.NotFound;
            }
            else
            {
                throw new Exception($"Failed to query DisplayCatalog Endpoint: {SelectedEndpoint.ToString()} Status Code: {httpResponse.StatusCode} Returned Data: {await httpResponse.Content.ReadAsStringAsync()}");
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="Query">The raw search query</param>
        /// <param name="DeviceFamily">The wanted DeviceFamily, only supported apps/games will be returned</param>
        /// <returns>Instance of DCatSearch, containing the returned products.</returns>
        public async Task<DCatSearch> SearchDCATAsync(string Query, DeviceFamily deviceFamily)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            HttpRequestMessage httpRequestMessage;
            switch (deviceFamily)
            {
                case DeviceFamily.Desktop:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Desktop");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Xbox:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Xbox");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Universal:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Universal");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Mobile:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Mobile");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.HoloLens:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Holographic");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.IotCore:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Iot");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.ServerCore:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Server");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Andromeda:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.8828080");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.WCOS:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Core");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
            }
            if (httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync();
                Result = DisplayCatalogResult.Found;
                DCatSearch dcatSearch = DCatSearch.FromJson(content);
                return dcatSearch;
            }
            else
            {
                throw new Exception($"Failed to search DisplayCatalog: {DeviceFamily.ToString()} Status Code: {httpResponse.StatusCode} Returned Data: {await httpResponse.Content.ReadAsStringAsync()}");
            }
        }
        
        public async Task<List<Addon>> GetAddonsForProductAsync()
        {
            if(this.IsFound == false)
            {
                throw new Exception("Can not search for addons on a non-existant product.");
            }
            List<Addon> ProductAddons = new List<Addon>();
            List<string> ProductIDs = new List<string>();
            throw new NotImplementedException();
            foreach(dynamic AddonData in this.ProductListing.Product.MarketProperties[0].RelatedProducts)
            {
                
            }

        }
        
        /// <summary>
        ///  
        /// </summary>
        /// <param name="Query">The raw search query</param>
        /// <param name="DeviceFamily">The wanted DeviceFamily, only supported apps/games will be returned</param>
        /// <param name="SkipCount">The number of results to skip, only 100 products will be returned at once. A skip count of 100 will return products starting with the 101st result.</param>
        /// <returns></returns>
        public async Task<DCatSearch> SearchDCATAsync(string Query, DeviceFamily DeviceFamily, int SkipCount)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            HttpRequestMessage httpRequestMessage;
            switch (DeviceFamily)
            {
                case DeviceFamily.Desktop:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Desktop");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Xbox:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Xbox");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Universal:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Universal");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Mobile:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Mobile");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.HoloLens:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Holographic");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.IotCore:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Iot");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.ServerCore:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.Server");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
                case DeviceFamily.Andromeda:
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{Utilities.TypeHelpers.EnumToSearchUri(SelectedEndpoint)}{Query}&productFamilyNames=apps,games&platformDependencyName=Windows.8828080");
                    httpResponse = await _httpClient.SendAsync(httpRequestMessage, new System.Threading.CancellationToken());
                    break;
            }
            if (httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync();
                Result = DisplayCatalogResult.Found;
                DCatSearch dcatSearch = DCatSearch.FromJson(content);
                return dcatSearch;
            }
            else
            {
                throw new Exception($"Failed to search DisplayCatalog: {DeviceFamily.ToString()} Status Code: {httpResponse.StatusCode} Returned Data: {await httpResponse.Content.ReadAsStringAsync()}");
            }
        }
    }

    
}

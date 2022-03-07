using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using FluentStore.SDK;
using FluentStore.SDK.Models;
using FluentStore.SDK.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace FluentStore.Sources.MicrosoftStore
{
    public class MicrosoftStoreHandler : PackageHandlerBase
    {
        private readonly StorefrontApi StorefrontApi = new();

        public const string NAMESPACE_MSSTORE = "microsoft-store";
        public const string NAMESPACE_MODERNPACK = "win-modern-package";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_MSSTORE,
            NAMESPACE_MODERNPACK,
        };

        public override string DisplayName => "Microsoft Store";

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            var packages = new List<PackageBase>();
            var page = (await StorefrontApi.GetHomeSpotlight(options: GetSystemOptions())).Payload;
            packages.AddRange(
                page.Cards.Where(card => card.ProductId.Length == 12 && card.TypeTag == "app")
                          .Select(card => new MicrosoftStorePackage(card) { Status = PackageStatus.BasicDetails })
            );

            return packages;
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();

            var page = (await StorefrontApi.Search(query, "apps", GetSystemOptions())).Payload;
            packages.AddRange(
                page.SearchResults.Select(card => new MicrosoftStorePackage(card) { Status = PackageStatus.BasicDetails })
            );

            for (int p = 1; p < 3 && page.NextUri != null; p++)
            {
                page = (await StorefrontApi.NextSearchPage(page)).Payload;
                packages.AddRange(
                    page.SearchResults.Select(card => new MicrosoftStorePackage(card) { Status = PackageStatus.BasicDetails })
                );
            }

            return packages;
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var suggs = await StorefrontApi.GetSearchSuggestions(query, GetSystemOptions());
            var packages = new List<PackageBase>();
            packages.AddRange(
                suggs.Payload.AssetSuggestions.Select(summ => new MicrosoftStorePackage(summary: summ) { Status = PackageStatus.BasicDetails })
            );

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            CatalogIdType idType = packageUrn.NamespaceIdentifier switch
            {
                NAMESPACE_MSSTORE => CatalogIdType.ProductId,
                NAMESPACE_MODERNPACK => CatalogIdType.PackageFamilyName,

                _ => throw new System.ArgumentException("URN not recognized", nameof(packageUrn))
            };

            string catalogId = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            return await GetPackageFromPage(catalogId, idType);
        }

        private async Task<MicrosoftStorePackage> GetPackageFromPage(string catalogId, CatalogIdType idType)
        {
            var page = await StorefrontApi.GetPage(catalogId, idType, GetSystemOptions());

            if (!page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V3.ProductDetails>(out var details))
            {
                // The Storefront API doesn't return 404 if the request was valid
                // but no such produdct exists, so it has to be caught manually.
                if (page.TryGetPayload<Microsoft.Marketplace.Storefront.Contracts.V1.ErrorResponse>(out var error))
                {
                    throw WebException.Create(404, error.ErrorDescription);
                }
                return null;
            }

            var package = new MicrosoftStorePackage(product: details);
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
            Regex rx = new(@"^https?:\/\/(?:www\.)?microsoft\.com\/(?:(?<locale>[a-z]{2}-[a-z]{2})\/)?(?:store\/(?:apps|productId)|(?:p|store\/r)(?:\/.+)?)\/(?<id>\w{12})",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = rx.Match(url.ToString());
            if (!m.Success)
                return null;

            return await GetPackage(Urn.Parse($"urn:{NAMESPACE_MSSTORE}:{m.Groups["id"]}"));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is not MicrosoftStorePackage msPackage)
                throw new System.ArgumentException("Must be a " + nameof(MicrosoftStorePackage), nameof(package));
            return "https://www.microsoft.com/store/apps/" + msPackage.StoreId;
        }

        private RequestOptions GetSystemOptions()
        {
            RequestOptions options = new();

            // Get system information
            options.DeviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            var sysArch = Win32Helper.GetSystemArchitecture();
            options.DeviceArchitecture = sysArch switch
            {
                Architecture.Arm32 => "arm",

                Architecture.Neutral or
                Architecture.Unknown => "x86",

                _ => sysArch.ToString()
            };

            // Get user token if available
            if (AccSvc.TryGetAuthenticatedHandler<Users.MicrosoftAccountHandler>(out var accHandler))
            {
                options.Token = accHandler.GetToken();
            }

            return options;
        }
    }
}

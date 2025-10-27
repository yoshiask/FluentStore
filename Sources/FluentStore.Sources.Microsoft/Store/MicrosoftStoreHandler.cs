﻿using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using FluentStore.SDK;
using FluentStore.SDK.Models;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V1;

namespace FluentStore.Sources.Microsoft.Store
{
    public partial class MicrosoftStoreHandler : PackageHandlerBase<Users.MicrosoftAccountHandler>
    {
        internal readonly StorefrontApi StorefrontApi = new();
        internal readonly StoreEdgeFDApi StoreEdgeFDApi = new();

        public const string NAMESPACE_MSSTORE = "microsoft-store";
        public const string NAMESPACE_COLLECTION = "microsoft-store-collection";
        public const string NAMESPACE_MODERNPACK = "win-modern-package";

        public MicrosoftStoreHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            AccountHandler = new Users.MicrosoftAccountHandler(passwordVaultService);
        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_MSSTORE,
            NAMESPACE_COLLECTION,
            NAMESPACE_MODERNPACK,
        };

        public override string DisplayName => "Microsoft Store";

        public override async IAsyncEnumerable<PackageBase> GetFeaturedPackagesAsync()
        {
            var collection = await StorefrontApi.GetRecommendationCollection("topFree", "apps", options: GetSystemOptions());

            var packages = collection.Payload.Cards?
                .Where(card => card.ProductId.Length == 12)
                .Select(card => new MicrosoftStorePackage(this, card) { Status = PackageStatus.BasicDetails });

            if (packages is null)
                yield break;

            foreach (var package in packages)
                yield return package;
        }

        public override async IAsyncEnumerable<PackageBase> SearchAsync(string query)
        {
            var page = (await StorefrontApi.Search(query, "apps", GetSystemOptions())).Payload;

            while (page is not null)
            {
                foreach (var details in page.HighlightedResults)
                {
                    var package = await MicrosoftStorePackageBase.CreateAsync(this, details.ProductId, product: details);
                    package.Status = PackageStatus.BasicDetails;

                    yield return package;
                }
                
                foreach (var card in page.SearchResults)
                {
                    var package = await MicrosoftStorePackageBase.CreateAsync(this, card.ProductId, card);
                    package.Status = PackageStatus.BasicDetails;

                    yield return package;
                }

                page = page.NextUri is not null
                    ? (await StorefrontApi.NextSearchPage(page)).Payload
                    : null;
            }
        }

        public override async IAsyncEnumerable<PackageBase> GetSearchSuggestionsAsync(string query)
        {
            var suggs = await StorefrontApi.GetSearchSuggestions(query, GetSystemOptions());

            foreach (var summ in suggs.Payload.AssetSuggestions)
            {
                var package = await MicrosoftStorePackageBase.CreateAsync(this, summ.ProductId, summary: summ);
                package.Status = PackageStatus.BasicDetails;

                yield return package;
            }
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            string catalogId = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;

            if (packageUrn.NamespaceIdentifier == NAMESPACE_COLLECTION)
            {
                var response = await StorefrontApi.GetCollection(catalogId, GetSystemOptions());
                MicrosoftStoreCollection collectionPackage = new(this, response.Payload);

                if (status.IsAtLeast(PackageStatus.Details))
                {
                    foreach (var card in response.Payload.Cards)
                    {
                        var item = await MicrosoftStorePackageBase.CreateAsync(this, card.ProductId, card);
                        collectionPackage.Items.Add(item);
                    }
                }

                return collectionPackage;
            }
            else
            {
                CatalogIdType idType = packageUrn.NamespaceIdentifier switch
                {
                    NAMESPACE_MSSTORE => CatalogIdType.ProductId,
                    NAMESPACE_MODERNPACK => CatalogIdType.PackageFamilyName,

                    _ => throw new System.ArgumentException("URN not recognized", nameof(packageUrn))
                };

                return await GetPackageFromPage(catalogId, idType, status);
            }
        }

        public override async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            var collectionDetail = (await StorefrontApi.GetCollections(options: GetSystemOptions())).Payload;

            foreach (var card in collectionDetail.Cards)
            {
                yield return new MicrosoftStoreCollection(this, collectionCard: card)
                {
                    Status = PackageStatus.BasicDetails
                };
            }
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
            Match m = MsStoreAppsRegex().Match(url);
            if (!m.Success)
            {
                m = XboxGamesRegex().Match(url);
                if (!m.Success)
                    return null;
            }

            return await GetPackageFromPage(m.Groups["id"].Value, CatalogIdType.ProductId, PackageStatus.Details);
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            var id = package.Urn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            return $"https://apps.microsoft.com/detail/{id}";
        }

        public override async Task<ReviewSummary> GetReviewsAsync(PackageBase package)
        {
            if (package is not MicrosoftStorePackageBase msPkg)
                throw new System.ArgumentException($"{nameof(package)} must be of type {nameof(MicrosoftStorePackageBase)}.");

            // Get the rest of the reviews
            var options = GetSystemOptions();
            var allReviews = StorefrontApi.GetAllProductReviews(msPkg.StoreId, startAt: msPkg.ReviewSummary?.Reviews.Count() ?? 0, options: options);
            await msPkg.Update(allReviews);

            return msPkg.ReviewSummary;
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
            var accHandler = GetAccountHandler();
            if (accHandler.IsLoggedIn)
            {
                accHandler.AuthenticateRequest(options);
            }

            return options;
        }

        private async Task<MicrosoftStorePackageBase> GetPackageFromPage(string catalogId, CatalogIdType idType, PackageStatus status)
        {
            var options = GetSystemOptions();
            var page = await StorefrontApi.GetPage(catalogId, idType, options);

            if (!page.TryGetPayload<ProductDetails>(out var details))
            {
                // The Storefront API doesn't return 404 if the request was valid
                // but no such produdct exists, so it has to be caught manually.
                if (page.TryGetPayload<ErrorResponse>(out var error))
                {
                    throw WebException.Create(404, error.ErrorDescription);
                }
                return null;
            }

            MicrosoftStorePackageBase package = await MicrosoftStorePackageBase.CreateAsync(this, catalogId, product: details);
            if (page.TryGetPayload<RatingSummary>(out var ratingSummary))
            {
                package.Update(ratingSummary);

                if (page.TryGetPayload<ReviewList>(out var reviewList))
                {
                    package.Update(reviewList);
                }
            }

            package.Status = status;
            return package;
        }

        [GeneratedRegex(@"(?:https?:\/\/)?(?:(?:www\.)?microsoft\.com\/(?:(?<locale>[a-z]{2}-[a-z]{2})\/)?(?:store\/(?:apps|productId)|(?:p|store\/r)(?:\/.+)?)|apps\.microsoft\.com\/(?:store\/)?detail(?:\/[\w-]+)?)\/(?<id>\w{12})", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
        private static partial Regex MsStoreAppsRegex();

        [GeneratedRegex(@"^(?:https?:\/\/)?(?:www\.)?xbox\.com\/(?:(?<locale>[a-z]{2}-[a-z]{2})\/)?(?:games\/store)(?:\/.+)?\/(?<id>\w{12})")]
        private static partial Regex XboxGamesRegex();
    }
}

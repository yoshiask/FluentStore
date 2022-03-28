﻿using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentStore.SDK;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Linq;
using FluentStore.SDK.Helpers;
using FluentStore.Services;

namespace FluentStore.Sources.FluentStore
{
    public class FluentStoreHandler : PackageHandlerBase
    {
        public const string NAMESPACE_COLLECTION = "fluent-store-collection";

        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public FluentStoreHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            AccountHandler = new Users.FluentStoreAccountHandler(FSApi, passwordVaultService);
        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_COLLECTION,
        };

        public override string DisplayName => "Fluent Store";

        public override Task<List<PackageBase>> GetFeaturedPackagesAsync() => Task.FromResult(_emptyPackageList);

        public override async Task<PackageBase> GetPackage(Urn urn, PackageStatus status)
        {
            if (urn.NamespaceIdentifier == NAMESPACE_COLLECTION)
            {
                string[] id = urn.GetContent<NamespaceSpecificString>().UnEscapedValue.Split(':');
                string userId = id[0];
                string collId = id[1];

                var collection = await FSApi.GetCollectionAsync(userId, collId);
                var collectionPack = new CollectionPackage(collection)
                {
                    Status = PackageStatus.BasicDetails
                };

                if (status.IsAtLeast(PackageStatus.Details))
                {
                    var items = new List<PackageBase>(collection.Items.Count);
                    foreach (string packageId in collection.Items)
                    {
                        // Get details for each item
                        Urn packageUrn = Urn.Parse(packageId);
                        PackageBase package = await PackageService.GetPackageAsync(packageUrn, PackageStatus.BasicDetails);
                        items.Add(package);
                    }
                    collectionPack.Update(items);

                    var authorProfile = await FSApi.GetUserProfileAsync(userId);
                    collectionPack.Update(authorProfile);

                    collectionPack.Status = PackageStatus.Details;
                }
                
                return collectionPack;
            }

            return null;
        }

        public override Task<List<PackageBase>> GetSearchSuggestionsAsync(string query) => Task.FromResult(_emptyPackageList);

        public override Task<List<PackageBase>> SearchAsync(string query) => Task.FromResult(_emptyPackageList);

        public override async Task<List<PackageBase>> GetCollectionsAsync()
        {
            // Get current user
            if (!AccountHandler.IsLoggedIn)
                return _emptyPackageList;

            var collections = await FSApi.GetCollectionsAsync(AccountHandler.CurrentUser.Id);
            return collections.Select(c => (PackageBase)new CollectionPackage(c) { Status = PackageStatus.BasicDetails }).ToList();
        }

        public override ImageBase GetImage() => GetImageStatic();
        public static ImageBase GetImageStatic()
        {
            return new FileImage
            {
                Url = "ms-appx:///Assets/Square71x71Logo.png"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            // Fluent Store does not have a website
            return null;
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            return "fluentstore://package/" + package.Urn.ToString();
        }
    }
}

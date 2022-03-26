using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSAPI = FluentStoreAPI.FluentStoreAPI;
using FluentStore.SDK;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Linq;
using FluentStore.SDK.Users;
using FluentStore.SDK.Helpers;

namespace FluentStore.Sources.FluentStore
{
    public class FluentStoreHandler : PackageHandlerBase
    {
        internal readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

        public const string NAMESPACE_COLLECTION = "fluent-store-collection";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_COLLECTION,
        };

        public override string DisplayName => "Fluent Store";

        internal Users.FluentStoreAccountHandler AccHandler { get; private set; }

        public override Task<List<PackageBase>> GetFeaturedPackagesAsync() => Task.FromResult(_emptyPackageList);

        public override async Task<PackageBase> GetPackage(Urn urn, PackageStatus status)
        {
            if (urn.NamespaceIdentifier == NAMESPACE_COLLECTION)
            {
                string[] id = urn.GetContent<NamespaceSpecificString>().UnEscapedValue.Split(':');
                string userId = id[0];
                string collId = id[1];

                var collection = await FSApi.GetCollectionAsync(userId, collId);
                var collectionPack = new CollectionPackage(this, collection)
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
                        PackageBase package = await PkgSvc.GetPackageAsync(packageUrn, PackageStatus.BasicDetails);
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
            var accHandler = AccSvc.GetHandlerForNamespace<Users.FluentStoreAccountHandler>();
            if (!accHandler.IsLoggedIn)
                return _emptyPackageList;

            var collections = await FSApi.GetCollectionsAsync(accHandler.CurrentUser.Id);
            return collections.Select(c => new CollectionPackage(this, c) { Status = PackageStatus.BasicDetails }).Cast<PackageBase>().ToList();
        }

        public override async Task<PackageBase> CreateCollection()
        {
            CollectionPackage pkg = new(this, new());
            return pkg;
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

        public override void OnLoaded()
        {
            // Subscribe to login changes
            AccHandler = AccSvc.GetHandlerForNamespace<Users.FluentStoreAccountHandler>();
            AccHandler.OnLoginStateChanged += AccHandler_OnLoginStateChanged;

            // Call login changed handler to make sure the states are synced
            AccHandler_OnLoginStateChanged(AccHandler.IsLoggedIn, AccHandler.CurrentUser);
        }

        private void AccHandler_OnLoginStateChanged(bool isLoggedIn, Account currentUser)
        {
            CanCreateCollections = isLoggedIn;
        }
    }
}

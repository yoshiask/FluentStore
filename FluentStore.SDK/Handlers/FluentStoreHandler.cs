using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSAPI = FluentStoreAPI.FluentStoreAPI;

namespace FluentStore.SDK.Handlers
{
    public class FluentStoreHandler : PackageHandlerBase
    {
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public const string NAMESPACE_COLLECTION = "fluent-store-collection";
        public override HashSet<string> HandledNamespaces => new HashSet<string>
        {
            NAMESPACE_COLLECTION,
        };

        public override string DisplayName => "Fluent Store";

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            return new List<PackageBase>(0);
        }

        public override async Task<PackageBase> GetPackage(Urn urn)
        {
            if (urn.NamespaceIdentifier == NAMESPACE_COLLECTION)
            {
                string[] id = urn.GetContent<NamespaceSpecificString>().UnEscapedValue.Split(':');
                string userId = id[0];
                string collId = id[1];

                var collection = await FSApi.GetCollectionAsync(userId, collId);
                var authorProfile = await FSApi.GetUserProfileAsync(userId);
                var items = new List<PackageBase>(collection.Items.Count);
                foreach (string packageId in collection.Items)
                {
                    // Get details for each item
                    Urn packageUrn = Urn.Parse(packageId);
                    PackageBase package = await PackageService.GetPackageAsync(packageUrn);
                    items.Add(package);
                }

                var collectionPack = new CollectionPackage(Image, collection, items);
                collectionPack.Update(authorProfile);
                return collectionPack;
            }

            return null;
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            // TODO
            return new List<PackageBase>();
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            // TODO
            return new List<PackageBase>();
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

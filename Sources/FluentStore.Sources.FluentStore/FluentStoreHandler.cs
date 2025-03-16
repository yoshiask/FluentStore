using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentStore.SDK;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Linq;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using FluentStore.SDK.AbstractUI.Models;
using OwlCore.AbstractUI.Models;
using System;
using FluentStore.Sources.FluentStore.Users;
using FluentStoreAPI;

namespace FluentStore.Sources.FluentStore
{
    public class FluentStoreHandler : PackageHandlerBase
    {
        public const string NAMESPACE_COLLECTION = "fluent-store-collection";

        private readonly FluentStoreApiClient FSApi;
        private readonly PackageService PackageService;
        private const string NameBoxId = "NameBox";
        private const string ImageUrlBoxId = "ImageUrlBox";
        private const string TileGlyphBoxId = "TileGlyphBox";
        private const string DescriptionBoxId = "DescriptionBox";
        private const string IsPublicSwitchId = "IsPublicSwitch";

        public FluentStoreHandler(PackageService pkgSvc, IPasswordVaultService passwordVaultService, ICommonPathManager pathManager)
            : base(passwordVaultService)
        {
            FSApi = new();
            PackageService = pkgSvc;
            AccountHandler = new FluentStoreAccountHandler(FSApi, passwordVaultService, pathManager);
        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_COLLECTION,
        };

        public override string DisplayName => "Fluent Store";

        public override async Task<PackageBase> GetPackage(Urn urn, PackageStatus status)
        {
            if (urn.NamespaceIdentifier == NAMESPACE_COLLECTION)
            {
                var collId = Guid.Parse(urn.GetContent<NamespaceSpecificString>().UnEscapedValue);

                var collection = await FSApi.GetCollectionAsync(collId);
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
                        PackageBase package = await PackageService.GetPackageAsync(packageUrn, PackageStatus.BasicDetails);
                        items.Add(package);
                    }
                    collectionPack.Update(items);

                    var authorProfile = await FSApi.GetCurrentUserProfileAsync();
                    collectionPack.Update(authorProfile);

                    collectionPack.Status = PackageStatus.Details;
                }
                
                return collectionPack;
            }

            return null;
        }

        public override async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            // Get current user
            if (!AccountHandler.IsLoggedIn)
                yield break;

            var collections = await FSApi.GetCollectionsAsync(CurrentAccount.Uuid);

            foreach (var coll in collections)
                yield return new CollectionPackage(this, coll) { Status = PackageStatus.BasicDetails };
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

        public override AbstractForm CreateEditForm(PackageBase package)
        {
            if (package is not CollectionPackage collectionPackage)
                return null;
            var collection = collectionPackage.Model;

            void CollectionEditForm_Submitted(object sender, EventArgs e)
            {
                if (sender is not AbstractForm form)
                    return;

                var nameBox = form.OfType<AbstractTextBox>().First(a => a.Id == NameBoxId);
                collection.Name = nameBox.Value;

                var imageUrlBox = form.OfType<AbstractTextBox>().First(a => a.Id == ImageUrlBoxId);
                collection.ImageUrl = imageUrlBox.Value;

                var tileGlyphBox = form.OfType<AbstractTextBox>().First(a => a.Id == TileGlyphBoxId);
                collection.TileGlyph = tileGlyphBox.Value;

                var descriptionBox = form.OfType<AbstractTextBox>().First(a => a.Id == DescriptionBoxId);
                collection.Description = descriptionBox.Value;

                var isPublicSwitch = form.OfType<AbstractBoolean>().First(a => a.Id == IsPublicSwitchId);
                collection.IsPublic = isPublicSwitch.State;
            }

            AbstractForm form = new("CollectionEditForm", "Save", onSubmit: CollectionEditForm_Submitted)
            {
                new AbstractTextBox(NameBoxId, collection.Name, "Name")
                {
                    Subtitle = "Name",
                    TooltipText = "The name of the collection.",
                },
                new AbstractTextBox(ImageUrlBoxId, collection.ImageUrl, "Image link")
                {
                    Subtitle = "Icon",
                    TooltipText = "A link to an image to be used as an icon.",
                },
                new AbstractTextBox(TileGlyphBoxId, collection.TileGlyph, "Short indicator")
                {
                    Subtitle = "Short indicator",
                    TooltipText = "Text to be shown on the collection's tile if no image is provided.",
                },
                new AbstractTextBox(DescriptionBoxId, collection.Description, "Description")
                {
                    Subtitle = "Description",
                    TooltipText = "A description of the collection.",
                },
                new AbstractBoolean(IsPublicSwitchId, "Public")
                {
                    State = collection.IsPublic,
                    TooltipText = "Determines if this collection is visible to others."
                }
            };
            form.Title = "Editing collection details";
            
            return form;
        }

        public override bool CanCreatePackage() => false;

        public override bool CanCreateCollection() => AccountHandler.IsLoggedIn;

        public override bool CanEditPackage(PackageBase package)
        {
            return package is CollectionPackage collectionPackage
                && AccountHandler.IsLoggedIn
                && collectionPackage.Model.AuthorId == CurrentAccount.Uuid;
        }

        public override bool CanEditCollection(PackageBase package) => CanEditPackage(package);

        public override bool CanDeletePackage(PackageBase package) => CanEditPackage(package);

        public override async Task<PackageBase> CreateCollectionAsync()
        {
            FluentStoreAPI.Models.Collection collection = new()
            {
                AuthorId = CurrentAccount.Uuid,
                IsPublic = true,
            };
            return new CollectionPackage(this, collection);
        }

        public override async Task<bool> SavePackageAsync(PackageBase package)
        {
            switch (package)
            {
                case CollectionPackage collectionPackage:
                    var updatedId = await FSApi.UpdateCollectionAsync(collectionPackage.Model);
                    if (updatedId is not null)
                    {
                        collectionPackage.Model.Id = updatedId.Value;
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    return false;
            }
        }

        public override async Task<bool> DeletePackageAsync(PackageBase package)
        {
            switch (package)
            {
                case CollectionPackage collectionPackage:
                    await FSApi.DeleteCollectionAsync(collectionPackage.Model.Id);
                    return true;

                default:
                    return false;
            }
        }

        private FluentStoreAccount CurrentAccount => (FluentStoreAccount)AccountHandler.CurrentUser;
    }
}

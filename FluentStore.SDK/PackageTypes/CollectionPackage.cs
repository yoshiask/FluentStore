using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStoreAPI.Models;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentStore.SDK.Packages
{
    public class CollectionPackage : PackageBase<Collection>
    {
        public CollectionPackage(Collection collection = null, IEnumerable<PackageBase> items = null)
        {
            if (collection != null)
                Update(collection);
            if (items != null)
                Update(items);
        }

        public void Update(Collection collection)
        {
            Guard.IsNotNull(collection, nameof(collection));
            Model = collection;

            // Set base properties
            Title = collection.Name;
            PublisherId = collection.AuthorId;
            //ReleaseDate = collection.LastUpdateDateUtc;
            Description = collection.Description;
            ShortTitle = Title;
        }

        public void Update(IEnumerable<PackageBase> items)
        {
            Guard.IsNotNull(items, nameof(items));

            foreach (PackageBase package in items)
                Items.Add(package);
        }

        public void Update(Profile author)
        {
            Guard.IsNotNull(author, nameof(author));

            // Set base properties
            DeveloperName = author.DisplayName;
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + Handlers.FluentStoreHandler.NAMESPACE_COLLECTION + ":" + PublisherId + ":" + Model.Id);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override async Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            if (folder == null)
                folder = await StorageHelper.CreatePackageDownloadFolder(Urn);
            DownloadItem = folder;

            bool success = true;
            foreach (PackageBase package in Items)
                success &= await package.DownloadPackageAsync(folder) != null;

            return success ? folder : null;
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            ImageBase image;
            if (string.IsNullOrEmpty(Model.ImageUrl))
            {
                if (!string.IsNullOrEmpty(Model.TileGlyph))
                {
                    image = new TextImage
                    {
                        Text = Model.TileGlyph,
                        ImageType = ImageType.Tile,
                    };
                }
                else
                {
                    image = TextImage.CreateFromName(Title, ImageType.Tile);
                }
            }
            else
            {
                image = new FileImage
                {
                    Url = Model.ImageUrl,
                    ImageType = ImageType.Logo,
                };
            }

            return image;
        }

        public override async Task<ImageBase> CacheHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            return new List<ImageBase>(0);
        }

        public override async Task<bool> InstallAsync()
        {
            bool success = true;
            foreach (PackageBase package in Items)
                success &= await package.InstallAsync();
            return success;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            bool isInstalled = true;
            foreach (PackageBase package in Items)
                isInstalled &= await package.CanLaunchAsync();
            return isInstalled;
        }

        public override async Task LaunchAsync()
        {
            // TODO: Would the user really want to open every single app in the collection?
            // Is there better UX for this?
            foreach (PackageBase package in Items)
                await package.LaunchAsync();
        }

        private ObservableCollection<PackageBase> _Items = new();
        [Display(Title = "Apps", Rank = 1)]
        public ObservableCollection<PackageBase> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }
    }
}

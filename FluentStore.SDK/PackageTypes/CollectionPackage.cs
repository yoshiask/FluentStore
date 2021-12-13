using FluentStore.SDK.Images;
using FluentStoreAPI.Models;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    public class CollectionPackage : GenericListPackage<Collection>
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
    }
}

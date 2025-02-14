using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.V4;
using CommunityToolkit.Diagnostics;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;

namespace FluentStore.Sources.Microsoft.Store
{
    public class MicrosoftStoreCollection : GenericPackageCollection<CollectionDetail>
    {
        public MicrosoftStoreCollection(PackageHandlerBase packageHandler, CollectionDetail collectionDetail = null, CardModel collectionCard = null)
            : base(packageHandler)
        {
            if (collectionDetail != null)
                Update(collectionDetail);

            if (collectionCard != null)
                Update(collectionCard);
        }

        public void Update(CollectionDetail collectionDetail)
        {
            Guard.IsNotNull(collectionDetail, nameof(collectionDetail));

            // Set base properties
            Title = collectionDetail.CuratedTitle;
            Description = collectionDetail.CuratedDescription;
            Price = 0.0;
            DisplayPrice = "View";
            Id = collectionDetail.Id;
            DeveloperName = "Microsoft";
            UpdateUrn();

            Images.Clear();
            Images.Add(new FileImage(collectionDetail.CuratedImageUrl)
            {
                ImageType = ImageType.Hero,
                BackgroundColor = collectionDetail.CuratedBGColor,
                ForegroundColor = collectionDetail.CuratedFGColor
            });
        }

        public void Update(CardModel collectionCard)
        {
            Guard.IsNotNull(collectionCard, nameof(collectionCard));
            
            Id = collectionCard.ProductId;
            Title = collectionCard.Title;
            Description = collectionCard.Description;
            UpdateUrn();

            Images.Clear();

            foreach (var image in collectionCard.Images)
            {
                Images.Add(new FileImage(image.Uri)
                {
                    ImageType = ImageType.Hero,
                    BackgroundColor = image.BackgroundColor,
                    ForegroundColor = image.ForegroundColor
                });
            }
        }

        private void UpdateUrn()
        {
            Urn = new(MicrosoftStoreHandler.NAMESPACE_COLLECTION, new RawNamespaceSpecificString(Id));
        }

        private string _id;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public override Task<ImageBase> CacheAppIcon() => CacheHeroImage();
    }
}

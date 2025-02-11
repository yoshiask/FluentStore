using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.V4;
using CommunityToolkit.Diagnostics;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.SDK.Packages;

namespace FluentStore.Sources.Microsoft.Store
{
    public class MicrosoftStoreCollection : GenericPackageCollection<CollectionDetail>
    {
        public MicrosoftStoreCollection(PackageHandlerBase packageHandler, CollectionDetail collectionDetail = null)
            : base(packageHandler)
        {
            if (collectionDetail != null)
                Update(collectionDetail);
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

            Items.Clear();
            foreach (var card in collectionDetail.Cards)
                Items.Add(MicrosoftStorePackageBase.Create(PackageHandler, card.ProductId, card));
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

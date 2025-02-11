using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.V4;
using CommunityToolkit.Diagnostics;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using System.Collections.Generic;

namespace FluentStore.Sources.Microsoft.Store
{
    public class MicrosoftStoreCollection : GenericPackageCollection<CollectionDetail>
    {
        private readonly object _cardItemLock = new();

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

            LoadCardsAsync(collectionDetail.Cards);
        }

        private void UpdateUrn()
        {
            Urn = new(MicrosoftStoreHandler.NAMESPACE_COLLECTION, new RawNamespaceSpecificString(Id));
        }

        private async Task LoadCardsAsync(IEnumerable<CardModel> cards)
        {
            Items.Clear();

            foreach (var card in cards)
            {
                var item = await MicrosoftStorePackageBase.CreateAsync(PackageHandler, card.ProductId, card);
                lock (_cardItemLock)
                {
                    Items.Add(item);
                }
            }
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

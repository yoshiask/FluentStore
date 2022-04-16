using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.V2;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V4;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Profile;
using System.IO;
using StoreDownloader;
using FluentStore.SDK;
using FluentStore.SDK.Packages;

namespace FluentStore.Sources.MicrosoftStore
{
    public class MicrosoftStoreCollection : GenericPackageCollection<CollectionDetail>
    {
        public MicrosoftStoreCollection(CollectionDetail collectionDetail = null)
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
                Items.Add(new MicrosoftStorePackage(card));

            Status = PackageStatus.DownloadReady;
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

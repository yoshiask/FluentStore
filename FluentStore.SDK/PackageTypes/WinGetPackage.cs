using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Marketplace.Storefront.Contracts.Enums;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Profile;
using WinGetRun;
using WinGetRun.Models;

namespace FluentStore.SDK.Packages
{
    public class WinGetPackage : PackageBase<Package>
    {
        private readonly WinGetApi WinGetApi = Ioc.Default.GetRequiredService<WinGetApi>();

        public WinGetPackage() { }
        public WinGetPackage(Package pack) => Update(pack);

        public void Update(Package pack)
        {
            Guard.IsNotNull(pack, nameof(pack));
            Model = pack;

            // Set base properties
            Title = pack.Latest.Name;
            PublisherId = pack.GetPublisherAndPackageIds().PublisherId;
            DeveloperName = pack.Latest.Publisher;
            ReleaseDate = pack.CreatedAt;
            Description = pack.Latest.Description;
            Version = pack.Versions[0];
            Website = pack.Latest.Homepage;

            // Set WinGet package properties
            PackageId = pack.GetPublisherAndPackageIds().PackageId;
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + Handlers.WinGetHandler.NAMESPACE_WINGET + ":" + Model.Id);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> PopulatePackageUri()
        {
            Manifest = await WinGetApi.GetManifest(PackageId, Version);
            PackageUri = new Uri(Manifest.Installers[0].Url);

            Status = PackageStatus.DownloadReady;
            return true;
        }

        public override async Task<ImageBase> GetAppIcon()
        {
            if (Model.IconUrl != null)
            {
                return new FileImage
                {
                    Url = Model.IconUrl,
                    ImageType = SDK.Images.ImageType.Logo
                };
            }
            else
            {
                return TextImage.CreateFromName(Model.Latest.Name);
            }
        }

        public override Task<ImageBase> GetHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> GetScreenshots()
        {
            return new List<ImageBase>();
        }

        public override Task<bool> InstallAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> IsPackageInstalledAsync()
        {
            throw new NotImplementedException();
        }

        public override Task LaunchAsync()
        {
            throw new NotImplementedException();
        }

        private Uri _PackageUri;
        public Uri PackageUri
        {
            get => _PackageUri;
            set => SetProperty(ref _PackageUri, value);
        }

        private string _PackageId;
        public string PackageId
        {
            get => _PackageId;
            set => SetProperty(ref _PackageId, value);
        }

        private Manifest _Manifest;
        public Manifest Manifest
        {
            get => _Manifest;
            set => SetProperty(ref _Manifest, value);
        }
    }
}

using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Storage;
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

        public override async Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            // Find the package URI
            if (!await PopulatePackageUri())
            {
                WeakReferenceMessenger.Default.Send(new PackageFetchFailedMessage(this, new Exception("An unknown error occurred.")));
                return null;
            }
            WeakReferenceMessenger.Default.Send(new PackageFetchCompletedMessage(this));

            // Download package
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);

            // Set the proper file name
            await DownloadItem.RenameAsync(System.IO.Path.GetFileName(PackageUri.ToString()), NameCollisionOption.ReplaceExisting);

            WeakReferenceMessenger.Default.Send(new PackageDownloadCompletedMessage(this, (StorageFile)DownloadItem));
            return DownloadItem;
        }

        private async Task<bool> PopulatePackageUri()
        {
            Manifest = await WinGetApi.GetManifest(Urn.GetContent<NamespaceSpecificString>().UnEscapedValue, Version);
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
                    ImageType = ImageType.Logo
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

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));

            // TODO: Use full trust component to start installer in slient mode
            return false;
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

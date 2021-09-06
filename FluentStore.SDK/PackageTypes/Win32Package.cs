using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentStore.SDK.Packages
{
    public class Win32Package<TModel> : PackageBase<TModel>
    {
        private Urn _Urn;
        public override Urn Urn
        {
            get => _Urn;
            set => _Urn = value;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            // TODO: How to check if an unpackaged app is installed?
            return false;
        }

        public override async Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            throw new NotImplementedException();
        }

        public override async Task<ImageBase> GetAppIcon()
        {
            return TextImage.CreateFromName(ShortTitle);
        }

        public override async Task<ImageBase> GetHeroImage()
        {
            return null;
        }

        public override async Task<List<ImageBase>> GetScreenshots()
        {
            return new List<ImageBase>(0);
        }

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsEqualTo((int)Status, (int)PackageStatus.Downloaded, nameof(Status));

            if (await Win32Helper.Install(this))
            {
                Status = PackageStatus.Installed;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task LaunchAsync()
        {
            // TODO: How to launch an unpackaged app?
        }
    }
}

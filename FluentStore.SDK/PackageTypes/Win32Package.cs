using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace FluentStore.SDK.Packages
{
    public class Win32Package<TModel> : PackageBase<TModel>
    {
        public Win32Package(PackageHandlerBase packageHandler) : base(packageHandler)
        {

        }

        public override async Task<bool> CanLaunchAsync()
        {
            // TODO: How to check if an unpackaged app is installed?
            return false;
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            throw new NotImplementedException();
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            return TextImage.CreateFromName(ShortTitle);
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

using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace FluentStore.SDK.Packages
{
    public class Win32Package<TModel>(PackageHandlerBase packageHandler) : PackageBase<TModel>(packageHandler)
    {
        public override async Task<bool> CanLaunchAsync()
        {
            // TODO: How to check if an unpackaged app is installed?
            return false;
        }

        public override Task<bool> CanDownloadAsync() => Task.FromResult(false);

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
            if (!IsDownloaded)
                await DownloadAsync();

            if (await Win32Helper.Install(this))
            {
                IsInstalled = true;
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

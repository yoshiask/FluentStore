using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Provides a default implementation of <see cref="PackageBase"/> that can be used
    /// by packages that do not have accessible installers (i.e. <see cref="Handlers.UwpCommunityHandler"/>.
    /// </summary>
    public class GenericPackage<TModel> : PackageBase<TModel>
    {
        public override Urn Urn { get; set; }

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override Task<FileSystemInfo> DownloadPackageAsync(DirectoryInfo folder = null) => Task.FromResult<FileSystemInfo>(null);

        public override Task<ImageBase> CacheAppIcon()
        {
            return Task.FromResult(Images.FirstOrDefault(i => i.ImageType == ImageType.Logo));
        }

        public override Task<ImageBase> CacheHeroImage()
        {
            return Task.FromResult(Images.FirstOrDefault(i => i.ImageType == ImageType.Hero));
        }

        public override Task<List<ImageBase>> CacheScreenshots()
        {
            return Task.FromResult(Images.Where(i => i.ImageType == ImageType.Screenshot).ToList());
        }

        public override Task<bool> InstallAsync() => throw new NotImplementedException();

        public override Task LaunchAsync() => throw new NotImplementedException();
    }
}

using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
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

        public override Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null) => Task.FromResult<IStorageItem>(null);

        public override Task<ImageBase> GetAppIcon()
        {
            return Task.FromResult(Images.FirstOrDefault(i => i.ImageType == ImageType.Logo));
        }

        public override Task<ImageBase> GetHeroImage()
        {
            return Task.FromResult(Images.FirstOrDefault(i => i.ImageType == ImageType.Hero));
        }

        public override Task<List<ImageBase>> GetScreenshots()
        {
            return Task.FromResult(Images.FirstOrDefault(i => i.ImageType == ImageType.Screenshot));
        }

        public override Task<bool> InstallAsync() => throw new NotImplementedException();

        public override Task LaunchAsync() => throw new NotImplementedException();
    }
}

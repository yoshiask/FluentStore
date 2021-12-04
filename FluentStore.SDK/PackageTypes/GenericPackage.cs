using CommunityToolkit.Diagnostics;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Provides a default implementation of <see cref="PackageBase"/> that can be used
    /// by packages that do not have accessible installers.
    /// </summary>
    public class GenericPackage<TModel> : PackageBase<TModel>
    {
        public override Urn Urn { get; set; }

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override async Task<FileSystemInfo> DownloadPackageAsync(DirectoryInfo folder = null)
        {
            await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);

            // Check for success
            if (Status.IsLessThan(PackageStatus.Downloaded))
                return null;

            if (PackageUri != null && DownloadItem is FileInfo file)
                file.Rename(Path.GetFileName(PackageUri.AbsolutePath));

            return DownloadItem;
        }

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

        public override async Task<bool> InstallAsync()
        {
            // A very basic method of installing a package. It has issues, for example Win32 installers
            // won't be run silently, but it's better than an error.

            // Make sure installer is downloaded
            Guard.IsTrue(Status.IsAtLeast(PackageStatus.Downloaded), nameof(Status));

            bool success = false;
            Models.InstallerType typeReduced = Type.Reduce();
            if (typeReduced == Models.InstallerType.Win32)
            {
                success = await Win32Helper.Install(this);
            }
            else if (typeReduced == Models.InstallerType.Msix)
            {
                success = await PackagedInstallerHelper.Install(this);
            }

            if (success)
                Status = PackageStatus.Installed;
            return success;
        }

        public override Task LaunchAsync() => throw new NotImplementedException();
    }
}

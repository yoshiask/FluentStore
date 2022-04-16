using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// A base class for packages that represent a list of pacakges.
    /// For example, Fluent Store's <c>CollectionPackage</c> inherits this class
    /// and <c>UwpCommunityPackage</c> uses it to represent Launch events.
    /// </summary>
    public class GenericPackageCollection<TModel> : PackageBase<TModel>, IPackageCollection
    {
        public GenericPackageCollection(PackageHandlerBase packageHandler) : base(packageHandler)
        {

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

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            if (folder == null)
                folder = StorageHelper.CreatePackageDownloadFolder(Urn);
            DownloadItem = folder;

            bool success = true;
            foreach (PackageBase package in Items)
                success &= await package.DownloadAsync(folder) != null;

            return success ? folder : null;
        }

        public override async Task<bool> InstallAsync()
        {
            bool success = true;
            foreach (PackageBase package in Items)
                success &= await package.InstallAsync();
            return success;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            bool isInstalled = true;
            foreach (PackageBase package in Items)
                isInstalled &= await package.CanLaunchAsync();
            return isInstalled;
        }

        public override async Task LaunchAsync()
        {
            // TODO: Would the user really want to open every single app in the collection?
            // Is there better UX for this?
            foreach (PackageBase package in Items)
                await package.LaunchAsync();
        }

        private ObservableCollection<PackageBase> _Items = new();
        [Display(Title = "Apps", Rank = 1)]
        public ObservableCollection<PackageBase> Items
        {
            get => _Items;
            set => SetProperty(ref _Items, value);
        }
    }
}

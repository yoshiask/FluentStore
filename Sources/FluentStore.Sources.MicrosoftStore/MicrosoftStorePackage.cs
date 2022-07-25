using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using StoreDownloader;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore
{
    public class MicrosoftStorePackage : MicrosoftStorePackageBase
    {
        public MicrosoftStorePackage(PackageHandlerBase packageHandler, CardModel card = null, ProductSummary summary = null, ProductDetails product = null) : base(packageHandler, card, summary, product)
        {
            Guard.IsFalse(IsWinGet);
        }

        protected override async Task<FileInfo> InternalDownloadAsync(DirectoryInfo folder)
        {
            try
            {
                // Get system and package info
                string[] categoryIds = new[] { Model.Skus[0].FulfillmentData.WuCategoryId };
                var sysInfo = Handlers.MicrosoftStore.Win32Helper.GetSystemInfo();
                folder ??= StorageHelper.GetTempDirectory();

                // Get update data
                WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
                var updates = await WindowsUpdateLib.FE3Handler.GetUpdates(categoryIds, sysInfo, "", WindowsUpdateLib.FileExchangeV3UpdateFilter.Application);
                if (updates == null || !updates.Any())
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(
                        WebException.Create(404, "No packages are available for " + ShortTitle), this, ErrorType.PackageFetchFailed));
                    return null;
                }
                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));

                // Set up progress handler
                void DownloadProgress(DownloadLib.GeneralDownloadProgress progress)
                {
                    var status = progress.DownloadedStatus[0];
                    WeakReferenceMessenger.Default.Send(
                        new PackageDownloadProgressMessage(this, status.DownloadedBytes, status.File.FileSize));
                }

                // Start download
                WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(this));
                string[] files = await MSStoreDownloader.DownloadPackageAsync(updates, folder, new DownloadProgress(DownloadProgress));
                if (files == null || files.Length == 0)
                    throw new Exception("Failed to download pacakges using WindowsUpdateLib");
                Status = InternalPackage.Status = PackageStatus.Downloaded;

                FileInfo downloadFile = new(Path.Combine(folder.FullName, files[0]));
                InternalPackage.DownloadItem = downloadFile;
                await ((ModernPackage<ProductDetails>)InternalPackage).GetInstallerType();
                Type = InternalPackage.Type;

                return downloadFile;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageDownloadFailed));
                return null;
            }
        }

        protected override void PopulateInternalPackage(CardModel card)
        {
            var package = (ModernPackage<ProductDetails>)InternalPackage ?? new ModernPackage<ProductDetails>(PackageHandler);
            package.PackageFamilyName ??= card.PackageFamilyNames?[0];
            InternalPackage = package;
        }

        protected override void PopulateInternalPackage(ProductDetails product)
        {
            var package = (ModernPackage<ProductDetails>)InternalPackage ?? new ModernPackage<ProductDetails>(PackageHandler);
            package.PackageFamilyName = product.PackageFamilyNames?[0];
            package.PublisherDisplayName = product.PublisherName;
            InternalPackage = package;
        }
    }
}

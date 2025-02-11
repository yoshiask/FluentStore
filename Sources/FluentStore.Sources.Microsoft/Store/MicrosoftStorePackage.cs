using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using OwlCore.AbstractUI.Models;
using StoreDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads;

namespace FluentStore.Sources.Microsoft.Store
{
    public class MicrosoftStorePackage : MicrosoftStorePackageBase
    {
        public MicrosoftStorePackage(PackageHandlerBase packageHandler, CardModel card = null, ProductSummary summary = null, ProductDetails product = null) : base(packageHandler, card, summary, product)
        {
            Guard.IsFalse(IsWinGet);
        }

        public override Task<bool> CanDownloadAsync() =>
            Task.FromResult(Model?.Skus.FirstOrDefault()?.FulfillmentData?.WuCategoryId is not null);

        protected async Task<FileSystemInfo> InternalDownloadAsync(DirectoryInfo folder, bool downloadEverything)
        {
            try
            {
                // Get system and package info
                string[] categoryIds = [Model.Skus[0].FulfillmentData.WuCategoryId];
                var sysInfo = SystemHelper.GetSystemInfo();
                folder ??= StorageHelper.GetTempDirectory().CreateSubdirectory(StorageHelper.PrepUrnForFile(Urn));

                // Get update data
                WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
                var updates = await FE3Handler.GetUpdates(categoryIds, sysInfo, "", FileExchangeV3UpdateFilter.Application);
                if (updates == null || !updates.Any())
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(
                        WebException.Create(404, "No packages are available for " + ShortTitle), this, ErrorType.PackageFetchFailed));
                    return null;
                }
                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));

                // Set up progress handler
                void DownloadProgress(GeneralDownloadProgress progress)
                {
                    var status = progress.DownloadedStatus[0];
                    WeakReferenceMessenger.Default.Send(
                        new PackageDownloadProgressMessage(this, status.DownloadedBytes, status.File.FileSize));
                }

                // Start download
                WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(this));
                string[] files = await MSStoreDownloader.DownloadPackageAsync(updates, folder, new DownloadProgress(DownloadProgress),
                    downloadAll: downloadEverything);
                if (files == null || files.Length == 0)
                    throw new Exception("Failed to download pacakges using WindowsUpdateLib");
                IsDownloaded = InternalPackage.IsDownloaded = true;

                if (downloadEverything)
                {
                    InternalPackage.DownloadItem = DownloadItem = folder;
                    Type = InstallerType.Unknown;
                }
                else
                {
                    InternalPackage.DownloadItem = DownloadItem
                        = new FileInfo(Path.Combine(folder.FullName, files[0]));

                    await ((ModernPackage<ProductDetails>)InternalPackage).GetInstallerType();
                    Type = InternalPackage.Type;
                }

                return DownloadItem;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageDownloadFailed));
                return null;
            }
        }

        protected override async Task<FileSystemInfo> InternalDownloadAsync(DirectoryInfo folder)
            => await InternalDownloadAsync(folder, false);

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

        public override List<AbstractButton> GetAdditionalCommands()
        {
            AbstractButton downloadEverything = new($"{Urn}_DownloadEverythingButton", "Download everything", "\uEA53")
            {
                TooltipText = "Downloads all available versions and dependencies",
            };
            downloadEverything.Clicked += DownloadEverything_Clicked;

            return new()
            {
                downloadEverything
            };
        }

        private async void DownloadEverything_Clicked(object sender, EventArgs e)
        {
            await InternalDownloadAsync(null, true);
        }
    }
}

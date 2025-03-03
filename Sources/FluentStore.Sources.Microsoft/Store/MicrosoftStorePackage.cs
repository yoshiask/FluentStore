using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using FluentStore.SDK.Packages;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using Microsoft.Msix.Utils.AppxPackaging;
using OwlCore.AbstractUI.Models;
using StoreDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads;

namespace FluentStore.Sources.Microsoft.Store
{
    public class MicrosoftStorePackage : MicrosoftStorePackageBase
    {
        private readonly HashSet<AppxPackageDependency> _dependencies = [];

        public MicrosoftStorePackage(PackageHandlerBase packageHandler, CardModel card = null, ProductSummary summary = null, ProductDetails product = null) : base(packageHandler, card, summary, product)
        {
            Guard.IsFalse(IsWinGet);
        }

        protected ModernPackage<ProductDetails> ModernPackage
        {
            get => (ModernPackage<ProductDetails>)InternalPackage;
            set => InternalPackage = value;
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
                var sysArch = Win32Helper.GetSystemArchitecture();

                folder ??= StorageHelper.GetTempDirectoryForPackage(this);

                // Get update data
                WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
                var allUpdates = await FE3Handler.GetUpdates(categoryIds, sysInfo, "", FileExchangeV3UpdateFilter.Application);
                if (allUpdates == null || !allUpdates.Any())
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(
                        WebException.Create(404, $"No packages are available for {ShortTitle}"), this, ErrorType.PackageFetchFailed));
                    return null;
                }

                var updates = allUpdates;
                if (!downloadEverything)
                {
                    // Filter out dependencies and parse package full name
                    var appUpdates = allUpdates
                        .Select(u => new
                        {
                            Update = u,
                            PackageFullName = PackageFullName.Parse(u.AppxMetadata.ContentPackageId)
                        })
                        .Where(a => !string.IsNullOrEmpty(a.PackageFullName.ResourceId));

                    // Rank app packages by architecture compatibility,
                    // then by package version
                    var rankedUpdates = InstallerSelection
                        .FilterAndRankByArchitecture(appUpdates,
                            a => a.PackageFullName.Architecture
                        )
                        .ThenByDescending(r => r.Installer.PackageFullName.Version);
                    
                    var update = rankedUpdates.FirstOrDefault()
                        ?? throw new Exception($"Failed to determine the best available installer for {ShortTitle}");

                    updates = [update.Installer.Update];
                }

                var uupFiles = await MSStoreDownloader.FetchFilesAsync(updates, includeDeps: true);

                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));

                // Set up progress handler
                Progress<GeneralDownloadProgress> progress = new(DownloadProgress);
                void DownloadProgress(GeneralDownloadProgress progressUpdate)
                {
                    WeakReferenceMessenger.Default.Send(
                        new PackageDownloadProgressMessage(this, progressUpdate.DownloadedTotalBytes, progressUpdate.EstimatedTotalBytes));
                }

                // Start download
                WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(this));
                var files = await MSStoreDownloader.DownloadFilesAsync(uupFiles, folder, progress);
                if (files == null || files.Length == 0)
                    throw new Exception("Failed to download packages using WindowsUpdateLib");

                IsDownloaded = InternalPackage.IsDownloaded = true;

                if (downloadEverything)
                {
                    InternalPackage.DownloadItem = DownloadItem = folder;
                    Type = InstallerType.Unknown;
                }
                else
                {
                    var mainPackageFileInfo = new FileInfo(Path.Combine(folder.FullName, files[0]));
                    InternalPackage.DownloadItem = DownloadItem = mainPackageFileInfo;

                    // Determine whether package is a bundle or not
                    Type = InstallerTypes.FromExtension(mainPackageFileInfo.Extension);
                    if (Type.Reduce() is not InstallerType.Msix)
                        throw new Exception($"Expected an MSIX-type installer, got {Type}");

                    // Enumerate dependencies by package family name and minimum version
                    IStream appxStream;
                    
                    if (Type.HasFlag(InstallerType.Bundle))
                    {
                        AppxBundleMetadata appxBundleMetadata = new(mainPackageFileInfo.FullName);

                        var rankedChildPackages = InstallerSelection
                            .FilterAndRankByArchitecture(
                                appxBundleMetadata.ChildAppxPackages.Where(p => p.ResourceId is null),
                                p => Enum.Parse<Architecture>(p.Architecture, true),
                                sysArch
                            )
                            .ThenByDescending(r => r.Installer.Version);

                        var selectedChildInstaller = rankedChildPackages.FirstOrDefault()
                            ?? throw new Exception($"Bundle does not contain a package compatible with {sysArch}");

                        ChildPackageMetadata selectedChildPackage = selectedChildInstaller.Installer;

                        appxStream = appxBundleMetadata.AppxBundleReader
                            .GetPayloadPackage(selectedChildPackage.RelativeFilePath)
                            .GetStream();
                    }
                    else
                    {
                        appxStream = global::Microsoft.Msix.Utils.StreamUtils.CreateInputStreamOnFile(mainPackageFileInfo.FullName);
                    }
                    
                    AppxMetadata appxMetadata = new(appxStream);
                    var appxArch = Enum.Parse<Architecture>(appxMetadata.Architecture);
                    var appxManifest = appxMetadata.AppxReader.GetManifest();
                    var appxDependenciesEnumerator = appxManifest.GetPackageDependencies();

                    _dependencies.Clear();
                    while (appxDependenciesEnumerator.GetHasCurrent())
                    {
                        var appxDependency = appxDependenciesEnumerator.GetCurrent();

                        _dependencies.Add(new(appxDependency, appxArch));

                        appxDependenciesEnumerator.MoveNext();
                    }

                    (appxStream as IDisposable)?.Dispose();

                    var updatePfns = allUpdates
                        .Select(u => PackageFullName.Parse(u.AppxMetadata.ContentPackageId))
                        .ToArray();

                    // Select which UUP updates to fetch
                    var depUpdates = allUpdates
                        .Select(u => new
                        {
                            PackageFullName = PackageFullName.Parse(u.AppxMetadata.ContentPackageId),
                            Update = u
                        })
                        .Where(u => _dependencies.Any(d => d.IsFullfilledBy(u.PackageFullName)))
                        .DistinctBy(u => u.PackageFullName.Name)
                        .Select(u => u.Update)
                        .ToList();

                    // Download dependencies
                    var dependencyDir = folder.CreateSubdirectory("deps");
                    uupFiles = await MSStoreDownloader.FetchFilesAsync(depUpdates, includeDeps: true);
                    files = await MSStoreDownloader.DownloadFilesAsync(uupFiles, dependencyDir, progress);
                    if (files == null || files.Length != _dependencies.Count)
                        throw new Exception("Failed to download package dependencies using WindowsUpdateLib");

                    // Ensure internal package knows about the dependencies
                    ModernPackage.DependencyDownloadItems = dependencyDir.EnumerateFileSystemInfos().ToList();
                }

                WeakReferenceMessenger.Default.Send(SuccessMessage.CreateForPackageDownloadCompleted(this));

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
            ModernPackage ??= new ModernPackage<ProductDetails>(PackageHandler);
            ModernPackage.PackageFamilyName ??= card.PackageFamilyNames?[0];
        }

        protected override void PopulateInternalPackage(ProductDetails product)
        {
            ModernPackage ??= new ModernPackage<ProductDetails>(PackageHandler);
            ModernPackage.PackageFamilyName = product.PackageFamilyNames?[0];
            ModernPackage.PublisherDisplayName = product.PublisherName;
        }

        public override List<AbstractButton> GetAdditionalCommands()
        {
            AbstractButton downloadEverything = new($"{Urn}_DownloadEverythingButton", "Download everything", "\uEA53")
            {
                TooltipText = "Downloads all available versions and dependencies",
            };
            downloadEverything.Clicked += DownloadEverything_Clicked;

            return [downloadEverything];
        }

        private async void DownloadEverything_Clicked(object sender, EventArgs e)
        {
            await InternalDownloadAsync(null, true);
        }
    }
}

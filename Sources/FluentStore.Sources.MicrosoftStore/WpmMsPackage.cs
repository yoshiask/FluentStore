using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.Sources.WinGet;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore
{
    public class WpmMsPackage : MicrosoftStorePackageBase
    {
        public WpmMsPackage(PackageHandlerBase packageHandler, CardModel card = null, ProductSummary summary = null, ProductDetails product = null) : base(packageHandler, card, summary, product)
        {
            Guard.IsTrue(IsWinGet);
        }

        public async Task Update(PackageManifestVersion manifest)
        {
            Guard.IsNotNull(manifest, nameof(manifest));
            Manifest = manifest;
            Version = manifest.PackageVersion;

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var installer = manifest.Installers.FirstOrDefault(i => i.InstallerLocale == culture.TwoLetterISOLanguageName && i.Markets.HasMarket(culture));
            PackageUri = installer.InstallerUri;
            Type = installer.InstallerType.ToSDKInstallerType();

            var internalPackage = (WinGetPackage)InternalPackage;
            //internalPackage.Installer = installer.ToWinGet();
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;
        }

        protected override async Task<FileSystemInfo> InternalDownloadAsync(DirectoryInfo folder)
        {
            // Find the package URI
            await PopulatePackageUri();
            if (Status.IsLessThan(PackageStatus.DownloadReady))
                return null;

            // Download package
            FileInfo downloadFile = (FileInfo)await InternalPackage.DownloadAsync(folder);
            Status = InternalPackage.Status;

            // Set the proper file type and extension
            string filename = Path.GetFileName(PackageUri.ToString());
            downloadFile.MoveRename(filename);
            return downloadFile;
        }

        private async Task GetWinGetPackage(string id)
        {

        }

        private async Task PopulatePackageUri()
        {
            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentUICulture;

                var api = ((MicrosoftStoreHandler)PackageHandler).StoreEdgeFDApi;
                Update((await api.GetPackageManifest(StoreId)).Data.Versions[0]);

                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));
                Status = InternalPackage.Status = PackageStatus.DownloadReady;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageFetchFailed));
            }
        }
    }
}

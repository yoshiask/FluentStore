using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.Sources.Microsoft.WinGet;
using Microsoft.Marketplace.Storefront.Contracts.V3;
using Microsoft.Marketplace.Storefront.Contracts.V8.One;
using Microsoft.Marketplace.Storefront.StoreEdgeFD.BusinessLogic.Response.PackageManifest;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.Sources.Microsoft.Store
{
    public class WpmMsPackage : MicrosoftStorePackageBase
    {
        public WpmMsPackage(PackageHandlerBase packageHandler, CardModel card = null, ProductSummary summary = null, ProductDetails product = null) : base(packageHandler, card, summary, product)
        {
            Guard.IsTrue(IsWinGet);
        }

        public void Update(PackageManifestVersion manifest)
        {
            Guard.IsNotNull(manifest, nameof(manifest));
            Manifest = manifest;
            Version = manifest.PackageVersion;

            var culture = System.Globalization.CultureInfo.CurrentCulture;
            var installer = manifest.Installers
                .OrderBy(RankInstaller)
                .FirstOrDefault();
            
            PackageUri = installer.InstallerUri;
            Type = installer.InstallerType.ToSDKInstallerType();

            var internalPackage = (WinGetPackage)InternalPackage;
            //internalPackage.Installer = installer.ToWinGet();
            CopyProperties(ref internalPackage);
            InternalPackage = internalPackage;

            int RankInstaller(SparkInstaller installer)
            {
                int rank = 0;
                System.Globalization.CultureInfo installerCulture = new(installer.InstallerLocale);

                if (installerCulture.ThreeLetterISOLanguageName == culture.ThreeLetterISOLanguageName)
                    rank += 1;

                if (installerCulture.LCID == culture.LCID)
                    rank *= 2;

                if (installer.Markets.HasMarket(culture))
                    rank *= 2;

                return rank;
            }
        }

        public override async Task<bool> CanDownloadAsync() => await GetPackageUri() is not null;

        protected override async Task<FileSystemInfo> InternalDownloadAsync(DirectoryInfo folder)
        {
            // Find the package URI
            await GetPackageUri();
            if (PackageUri is null)
                return null;

            // Download package
            FileInfo downloadFile = (FileInfo)await InternalPackage.DownloadAsync(folder);
            Status = InternalPackage.Status;

            // Set the proper file type and extension
            string filename = Path.GetFileName(PackageUri.ToString());
            downloadFile.MoveRename(filename);
            return downloadFile;
        }

        private async Task<Uri> GetPackageUri()
        {
            if (PackageUri is not null)
                return PackageUri;

            WeakReferenceMessenger.Default.Send(new PackageFetchStartedMessage(this));
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentUICulture;

                var api = ((MicrosoftStoreHandler)PackageHandler).StoreEdgeFDApi;
                var manifest = await api.GetPackageManifest(StoreId);

                Update(manifest.Data.Versions[0]);

                WeakReferenceMessenger.Default.Send(new SuccessMessage(null, this, SuccessType.PackageFetchCompleted));

                return PackageUri;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageFetchFailed));
            }

            return null;
        }
    }
}

using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Foundation;

namespace FluentStore.SDK.Helpers
{
    public static class StorageHelper
    {
        private static HttpClient _HttpClient;
        private static HttpClient HttpClient
        {
            get
            {
                if (_HttpClient == null)
                    _HttpClient = new();
                return _HttpClient;
            }
        }

        public static async Task<StorageFile> GetPackageFile(Urn packageUrn, StorageFolder folder = null)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = ApplicationData.Current.LocalCacheFolder;

            return await folder.GetFileAsync(PrepUrnForFile(packageUrn));
        }

        public static async Task<StorageFile> CreatePackageFile(Urn packageUrn, StorageFolder folder = null)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = ApplicationData.Current.LocalCacheFolder;

            return await folder.CreateFileAsync(PrepUrnForFile(packageUrn), CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<StorageFolder> CreatePackageDownloadFolder(Urn urn)
        {
            Guard.IsNotNull(urn, nameof(urn));

            return await CreateTempFolderAsync(PrepUrnForFile(urn));
        }

        public static async Task<StorageFolder> CreateTempFolderAsync(string relativePath)
        {
            return await EnsureExists(ApplicationData.Current.LocalCacheFolder, relativePath);
        }

        public static async Task<StorageFolder> EnsureExists(StorageFolder folder, string relativePath)
        {
            Guard.IsNotNull(folder, nameof(folder));

            StorageFolder prevFolder = folder;
            foreach (string fragment in relativePath.Split('/', '\\'))
            {
                StorageFolder curFolder = await prevFolder.TryGetItemAsync(fragment) as StorageFolder;
                if (curFolder == null)
                    curFolder = await prevFolder.CreateFolderAsync(fragment);
                prevFolder = curFolder;
            }

            return prevFolder;
        }

        public static string PrepUrnForFile(Urn urn)
        {
            string urnStr = urn.ToString()[4..];
            return urnStr.Replace(":", "_");
            byte[] hashBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(urnStr));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        public static async Task BackgroundDownloadPackage(PackageBase package, Uri downloadUri, StorageFolder folder = null)
        {
            // Create the location to download to
            StorageFile file = await CreatePackageFile(package.Urn, folder);
            package.DownloadItem = file;

            try
            {
                void DownloadProgress(IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> asyncInfo, HttpProgress p)
                {
                    if (p.TotalBytesToReceive.HasValue)
                        WeakReferenceMessenger.Default.Send(
                            new PackageDownloadProgressMessage(package, p.BytesReceived, p.TotalBytesToReceive.Value));
                }
                void DownloadComplete(IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> asyncInfo, AsyncStatus asyncStatus)
                {
                    switch (asyncStatus)
                    {
                        case AsyncStatus.Completed:
                            WeakReferenceMessenger.Default.Send(
                                new PackageDownloadProgressMessage(package, 1, 1));
                            package.Status = PackageStatus.Downloaded;
                            break;

                        case AsyncStatus.Error:
                            WeakReferenceMessenger.Default.Send(
                                new PackageDownloadFailedMessage(package, asyncInfo.ErrorCode));
                            break;

                        case AsyncStatus.Started:
                            WeakReferenceMessenger.Default.Send(
                                new PackageDownloadStartedMessage(package));
                            break;
                    }
                }

                // Prep download
                var operation = HttpClient.GetAsync(downloadUri);
                //operation.Completed = DownloadComplete;
                operation.Progress = DownloadProgress;

                // Start download
                HttpResponseMessage response = await operation;
                response.EnsureSuccessStatusCode();

                // Save to file
                using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                await response.Content.WriteToStreamAsync(fileStream);
                await fileStream.FlushAsync();

                package.Status = PackageStatus.Downloaded;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new PackageDownloadFailedMessage(package, ex));
                package.Status = PackageStatus.DownloadReady;
            }
        }
    }
}

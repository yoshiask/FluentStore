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
using System.IO;
using System.Linq;

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

        public static (FileInfo info, FileStream stream) GetPackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Open);

        public static (FileInfo info, FileStream stream) CreatePackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Create);

        public static (FileInfo info, FileStream stream) OpenPackageFile(Urn packageUrn, DirectoryInfo folder = null, FileMode mode = FileMode.Create)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = new(Path.GetTempPath());

            FileInfo file = new(Path.Combine(folder.FullName, PrepUrnForFile(packageUrn)));
            return (file, file.Open(mode));
        }

        public static DirectoryInfo CreatePackageDownloadFolder(Urn urn)
        {
            Guard.IsNotNull(urn, nameof(urn));

            return CreateTempFolderAsync(PrepUrnForFile(urn));
        }

        public static DirectoryInfo CreateTempFolderAsync(string relativePath)
        {
            return EnsureExists(new(Path.GetTempPath()), relativePath);
        }

        public static DirectoryInfo EnsureExists(DirectoryInfo folder, string relativePath)
        {
            Guard.IsNotNull(folder, nameof(folder));

            DirectoryInfo prevFolder = folder;
            foreach (string fragment in relativePath.Split('/', '\\'))
            {
                DirectoryInfo curFolder = prevFolder.EnumerateDirectories(fragment).FirstOrDefault();
                if (curFolder == null)
                    curFolder = prevFolder.CreateSubdirectory(fragment);
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

        public static async Task BackgroundDownloadPackage(PackageBase package, Uri downloadUri, DirectoryInfo folder = null)
        {
            // Create the location to download to
            (FileInfo info, FileStream stream) = CreatePackageFile(package.Urn, folder);
            package.DownloadItem = info;

            try
            {
                //void DownloadProgress(IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> asyncInfo, HttpProgress p)
                void DownloadProgress(HttpProgress p)
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
                var operation = HttpClient.GetAsync(downloadUri).AsTask(new Progress<HttpProgress>(DownloadProgress)).ConfigureAwait(false);
                //operation.Completed = DownloadComplete;
                //operation.Progress = DownloadProgress;

                // Start download
                HttpResponseMessage response = await operation;
                response.EnsureSuccessStatusCode();

                // Save to file
                using var contentStream = (await response.Content.ReadAsInputStreamAsync()).AsStreamForRead();
                await contentStream.CopyToAsync(stream);
                stream.Flush();
                stream.Dispose();

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

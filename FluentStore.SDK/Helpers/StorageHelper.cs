using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.IO;
using System.Linq;
using Windows.Storage.Streams;

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

        public static DirectoryInfo GetTempDirectoryPath()
        {
            DirectoryInfo tempDir = new(Path.Combine(Path.GetTempPath(), "FluentStoreBeta"));
            if (!tempDir.Exists)
                tempDir.Create();
            return tempDir;
        }

        public static (FileInfo info, FileStream stream) GetPackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Open);

        public static (FileInfo info, FileStream stream) CreatePackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Create);

        public static (FileInfo info, FileStream stream) OpenPackageFile(Urn packageUrn, DirectoryInfo folder = null, FileMode mode = FileMode.Create)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = GetTempDirectoryPath();

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
            return EnsureExists(GetTempDirectoryPath(), relativePath);
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
                ulong? length = null;
                void DownloadProgress(ulong progress)
                {
                    if (length.HasValue)
                        WeakReferenceMessenger.Default.Send(
                            new PackageDownloadProgressMessage(package, progress, length.Value));

                }

                // Start download
                HttpResponseMessage response = await HttpClient.GetAsync(downloadUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // this is horrible
                if (response.Content.TryComputeLength(out ulong computedLength) || (computedLength = response.Content.Headers.ContentLength.GetValueOrDefault()) != 0)
                {
                    length = computedLength;
                }

                // Save to file
                using IInputStream contentStream = await response.Content.ReadAsInputStreamAsync();
                using IRandomAccessStream outputStream = stream.AsRandomAccessStream();

                await RandomAccessStream.CopyAndCloseAsync(contentStream, outputStream)
                    .AsTask(new Progress<ulong>(DownloadProgress));

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

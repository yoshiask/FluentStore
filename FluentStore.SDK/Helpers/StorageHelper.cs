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

        public static FileInfo GetPackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Open);

        public static FileInfo CreatePackageFile(Urn packageUrn, DirectoryInfo folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Create);

        public static FileInfo OpenPackageFile(Urn packageUrn, DirectoryInfo folder = null, FileMode mode = FileMode.Create)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = GetTempDirectoryPath();

            return new(Path.Combine(folder.FullName, PrepUrnForFile(packageUrn)));
        }

        public static DirectoryInfo CreatePackageDownloadFolder(Urn urn)
        {
            Guard.IsNotNull(urn, nameof(urn));

            return CreateTempFolderAsync(PrepUrnForFile(urn));
        }

        public static DirectoryInfo CreateTempFolderAsync(string relativePath)
        {
            return GetTempDirectoryPath().EnsureExists(relativePath);
        }

        public static DirectoryInfo EnsureExists(this DirectoryInfo folder, string relativePath)
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

        public static void RecursiveDelete(this DirectoryInfo folder)
        {
            if (!folder.Exists) return;

            foreach (var dir in folder.EnumerateDirectories())
                dir.RecursiveDelete();
            folder.Delete(true);
        }

        public static void RecursiveDelete(this FileSystemInfo info)
        {
            if (info is DirectoryInfo subdir)
                subdir.RecursiveDelete();
            else
                info.Delete();
        }

        public static void MoveRename(this FileInfo file, string newName, bool overwrite = true)
        {
            string newPath = Path.Combine(file.DirectoryName, newName);
            file.MoveTo(newPath, overwrite);
        }

        public static FileInfo CopyRename(this FileInfo file, string newName, bool overwrite = true)
        {
            string newPath = Path.Combine(file.DirectoryName, newName);
            return file.CopyTo(newPath, overwrite);
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
            FileInfo info = null;
            FileStream stream = null;

            // Use cached download if available
            DownloadCache cache = new(folder);
            if (cache.TryGet(package.Urn, out var cacheEntry) && cacheEntry.HasValue && cacheEntry.Value.GetDownloadItem() is FileInfo cachedFile)
            {
                if (cacheEntry.Value.GetVersion() == package.Version)
                {
                    info = cachedFile;
                    goto downloaded;
                }
            }
            else
            {
                // Create the location to download to
                info = CreatePackageFile(package.Urn, folder);
                stream = info.OpenWrite();
                cache.Add(package.Urn, package.Version, info);
            }

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
                WeakReferenceMessenger.Default.Send(new PackageDownloadStartedMessage(package));
                HttpResponseMessage response = await HttpClient.GetAsync(downloadUri);
                if (!response.IsSuccessStatusCode)
                    throw new Models.WebException((int)response.StatusCode, response.ReasonPhrase);

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
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new PackageDownloadFailedMessage(package, ex));
                package.Status = PackageStatus.DownloadReady;
                stream?.Dispose();  // Make sure file is closed, or it will fail to remove from the cache
                cache.Remove(package.Urn);
                return;
            }
            finally
            {
                stream?.Dispose();
            }

        downloaded:
            package.DownloadItem = info;
            package.Status = PackageStatus.Downloaded;
        }
    }
}

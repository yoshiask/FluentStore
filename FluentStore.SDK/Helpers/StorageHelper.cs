using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Messages;
using Garfoot.Utilities.FluentUrn;
using OwlCore.Storage;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

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

        public static DirectoryInfo GetTempDirectory()
        {
            var pathManager = CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default.GetRequiredService<Services.ICommonPathManager>();
            return pathManager.GetTempDirectory();
        }

        public static FileInfo GetPackageFile(Urn packageUrn, DirectoryInfo folder = null)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            folder ??= GetTempDirectory();

            return new(Path.Combine(folder.FullName, PrepUrnForFile(packageUrn)));
        }

        public static DirectoryInfo CreatePackageDownloadFolder(Urn urn)
        {
            Guard.IsNotNull(urn, nameof(urn));

            return CreateTempFolderAsync(PrepUrnForFile(urn));
        }

        public static DirectoryInfo CreateTempFolderAsync(string relativePath)
        {
            return GetTempDirectory().EnsureExists(relativePath);
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

        public static async Task RecursiveDelete(this IStorable storable)
        {
            if (storable is IStorableChild storableChild)
            {
                var parent = await storableChild.GetParentAsync();
                if (parent is IModifiableFolder mutParent)
                    await mutParent.DeleteAsync(storableChild);
            }
        }

        public static async Task CreateRecursiveCopyOfAsync(this IModifiableFolder destinationFolder, IStorable storableToCopy, bool overwrite = false, bool extractToRoot = false, CancellationToken token = default)
        {
            if (storableToCopy is IFile file)
            {
                await destinationFolder.CreateCopyOfAsync(file, overwrite, token);
            }
            else if (storableToCopy is IFolder folder)
            {
                IModifiableFolder childDestinationFolder = extractToRoot
                    ? destinationFolder
                    : await destinationFolder.CreateFolderAsync(destinationFolder.Name, overwrite, token) as IModifiableFolder
                    ?? throw new InvalidOperationException("Recursive copying requires new folders in the destination to be modifiable.");

                await foreach (var childStorable in folder.GetItemsAsync(cancellationToken: token))
                    await childDestinationFolder.CreateRecursiveCopyOfAsync(childStorable, overwrite, false, token);
            }
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
            if (cache.TryGetFile(package.Urn, package.Version, out info))
            {
                goto downloaded;
            }
            else
            {
                // Create the location to download to
                info = GetPackageFile(package.Urn, folder);
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
                    throw Models.WebException.Create((int)response.StatusCode, response.ReasonPhrase, downloadUri.ToString());

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
                stream?.Dispose();
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, package, ErrorType.PackageDownloadFailed));
                package.Status = PackageStatus.DownloadReady;
                stream?.Dispose();  // Make sure file is closed, or it will fail to remove from the cache
                cache.Remove(package.Urn);
                return;
            }

        downloaded:
            package.DownloadItem = info;
            package.Status = PackageStatus.Downloaded;
        }

        public static DirectoryInfo ExtractArchiveToDirectory(FileInfo archiveFile, bool overwrite)
        {
            using FileStream archiveStream = archiveFile.OpenRead();
            using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read);
            var dir = new DirectoryInfo(archiveFile.FullName[..^archiveFile.Extension.Length]);
            archive.ExtractToDirectory(dir.FullName, overwrite);
            return dir;
        }

        internal static string GetFileId(FileSystemInfo file)
        {
            using var fs = File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Vanara.PInvoke.Kernel32.GetFileInformationByHandle(fs.SafeFileHandle, out var info);
            return $"{(info.nFileIndexHigh << 32) | info.nFileIndexLow:x}";
        }
    }
}

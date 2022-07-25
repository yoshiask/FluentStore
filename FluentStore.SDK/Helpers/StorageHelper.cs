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
using System.IO.Compression;
using OwlCore.AbstractStorage;
using FluentStore.Services;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FluentStore.SDK.Helpers
{
    public static class StorageHelper
    {
        private static readonly ICommonPathManager pathManager = Ioc.Default.GetRequiredService<ICommonPathManager>();

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

        public static Task<IFileData> GetPackageFile(Urn packageUrn, IFolderData folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Open);

        public static Task<IFileData> CreatePackageFile(Urn packageUrn, IFolderData folder = null)
            => OpenPackageFile(packageUrn, folder, FileMode.Create);

        public static async Task<IFileData> OpenPackageFile(Urn packageUrn, IFolderData folder = null, FileMode mode = FileMode.Create)
        {
            Guard.IsNotNull(packageUrn, nameof(packageUrn));
            if (folder == null)
                folder = await pathManager.GetTempDirectoryAsync();

            return await folder.CreateFileAsync(PrepUrnForFile(packageUrn));
        }

        public static Task<IFolderData> CreatePackageDownloadFolder(Urn urn)
        {
            Guard.IsNotNull(urn, nameof(urn));

            return pathManager.CreateDirectoryTempAsync(PrepUrnForFile(urn));
        }

        public static async Task<IFolderData> EnsureExists(this IFolderData folder, string relativePath)
        {
            Guard.IsNotNull(folder, nameof(folder));

            var dirSepChars = new[] { Path.DirectorySeparatorChar, '/', '\\' };
            IFolderData prevFolder = folder;
            foreach (string fragment in relativePath.Split(dirSepChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                prevFolder = await prevFolder.CreateFolderAsync(fragment);

            return prevFolder;
        }

        public static async Task RecursiveDelete(this IFolderData folder)
        {
            if (!Directory.Exists(folder.Path)) return;

            foreach (var dir in await folder.GetFoldersAsync())
                await dir.RecursiveDelete();

            await folder.DeleteAsync();
        }

        public static string PrepUrnForFile(Urn urn)
        {
            string urnStr = urn.ToString()[4..];
            return urnStr.Replace(":", "_");
        }

        public static async Task BackgroundDownloadPackage(PackageBase package, Uri downloadUri, IFolderData folder = null)
        {
            IFileData info = null;
            Stream stream = null;

            // Use cached download if available
            DownloadCache cache = new(folder);
            (bool isCached, var cacheEntry) = await cache.TryGet(package.Urn);
            if (isCached && cacheEntry.HasValue && cacheEntry.Value.GetDownloadItem() is IFileData cachedFile)
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
                info = await CreatePackageFile(package.Urn, folder);
                stream = await info.GetStreamAsync(FileAccessMode.ReadWrite);
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

        public static async Task<IFolderData> ExtractArchiveToDirectory(IFileData archiveFile, bool overwrite)
        {
            var parentDir = await archiveFile.GetParentAsync();

            using Stream archiveStream = await archiveFile.GetStreamAsync();
            using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read);

            var dir = await parentDir.CreateFolderAsync(archiveFile.Name);
            archive.ExtractToDirectory(dir.Path, overwrite);

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

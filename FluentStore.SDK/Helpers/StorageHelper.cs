using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FluentStore.SDK.Helpers
{
    public static class StorageHelper
    {
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
            string urnStr = urn.ToString().Substring(4);
            return urnStr.Replace(":", "_");
            byte[] hashBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(urnStr));
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}

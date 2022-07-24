// https://github.com/Arlodotexe/strix-music/blob/e7d2faee69420b6b4c75ece36aa2cbbedb34facb/src/Sdk/StrixMusic.Sdk.WinUI/Models/FolderData.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwlCore.Extensions;
using Windows.Storage;

namespace OwlCore.AbstractStorage
{
    /// <inheritdoc/>
    public class StorageFolderData : IFolderData
    {
        /// <summary>
        /// The underlying <see cref="StorageFolder"/> instance in use.
        /// </summary>
        public StorageFolder StorageFolder { get; private set; }

        /// <summary>
        /// Constructs a new instance of <see cref="IFolderData"/>.
        /// </summary>
        public StorageFolderData(StorageFolder folder)
        {
            StorageFolder = folder;
            Id = folder.Path.HashMD5Fast();
        }

        /// <inheritdoc/>
        public string Name => StorageFolder.Name;

        /// <inheritdoc/>
        public string Path => StorageFolder.Path;

        /// <inheritdoc/>
        public string? Id { get; set; }

        /// <inheritdoc/>
        public async Task<IEnumerable<IFileData>> GetFilesAsync()
        {
            var files = await StorageFolder.GetFilesAsync();

            return files.Select(x => new StorageFileData(x)).ToArray();
        }

        /// <inheritdoc />
        public Task DeleteAsync() => StorageFolder.DeleteAsync().AsTask();

        /// <inheritdoc/>
        public async Task<IFolderData?> GetParentAsync()
        {
            var storageFolder = await StorageFolder.GetParentAsync();

            return new StorageFolderData(storageFolder);
        }

        /// <inheritdoc/>
        public async Task<IFolderData> CreateFolderAsync(string desiredName)
        {
            var storageFolder = await StorageFolder.CreateFolderAsync(desiredName);

            return new StorageFolderData(storageFolder);
        }

        /// <inheritdoc/>
        public async Task<IFolderData> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            var collisionOptions = (Windows.Storage.CreationCollisionOption)Enum.Parse(typeof(Windows.Storage.CreationCollisionOption), options.ToString());

            var storageFolder = await StorageFolder.CreateFolderAsync(desiredName, collisionOptions);

            return new StorageFolderData(storageFolder);
        }

        /// <inheritdoc/>
        public async Task<IFileData> CreateFileAsync(string desiredName)
        {
            var storageFile = await StorageFolder.CreateFileAsync(desiredName);

            return new StorageFileData(storageFile);
        }

        /// <inheritdoc/>
        public async Task<IFileData> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            var collisionOptions = (Windows.Storage.CreationCollisionOption)Enum.Parse(typeof(Windows.Storage.CreationCollisionOption), options.ToString());
            var storageFile = await StorageFolder.CreateFileAsync(desiredName, collisionOptions);

            return new StorageFileData(storageFile);
        }

        /// <inheritdoc/>
        public async Task<IFolderData?> GetFolderAsync(string name)
        {
            var folderData = await StorageFolder.GetFolderAsync(name);

            return new StorageFolderData(folderData);
        }

        /// <inheritdoc/>
        public async Task<IFileData?> GetFileAsync(string name)
        {
            var fileData = await StorageFolder.GetFileAsync(name);

            return new StorageFileData(fileData);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IFolderData>> GetFoldersAsync()
        {
            var foldersData = await StorageFolder.GetFoldersAsync();

            return foldersData.Select(x => new StorageFolderData(x));
        }

        /// <inheritdoc />
        public async Task EnsureExists()
        {
            try
            {
                _ = StorageFolder.GetFolderFromPathAsync(StorageFolder.Path);
            }
            catch
            {
                StorageFolder = await StorageFolder.CreateFolderAsync(StorageFolder.Name);
            }
        }
    }
}

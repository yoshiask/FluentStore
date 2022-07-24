// https://github.com/Arlodotexe/strix-music/blob/e7d2faee69420b6b4c75ece36aa2cbbedb34facb/src/Sdk/StrixMusic.Sdk.WinUI/Models/FileData.cs

using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using OwlCore.Extensions;
using Windows.Storage;

namespace OwlCore.AbstractStorage
{
    /// <inheritdoc cref="IMutableFileData"/>
    public class StorageFileData : IMutableFileData
    {
        /// <summary>
        /// The underlying <see cref="StorageFile"/> instance in use.
        /// </summary>
        internal StorageFile StorageFile { get; }

        /// <summary>
        /// Creates a new instance of <see cref="StorageFileData" />.
        /// </summary>
        /// <param name="storageFile">The <see cref="StorageFile"/> to wrap.</param>
        public StorageFileData(StorageFile storageFile)
        {
            StorageFile = storageFile;
            Properties = new StorageFileDataProperties(storageFile);
            Id = StorageFile.Path.HashMD5Fast();
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Path => StorageFile.Path;

        /// <inheritdoc/>
        public string Name => StorageFile.Name;

        /// <inheritdoc/>
        public string DisplayName => StorageFile.DisplayName;

        /// <inheritdoc/>
        public string FileExtension => StorageFile.FileType;

        /// <inheritdoc/>
        public IFileDataProperties Properties { get; set; }

        /// <inheritdoc/>
        public async Task<IFolderData> GetParentAsync()
        {
            var storageFile = await StorageFile.GetParentAsync();

            Guard.IsNotNull(storageFile, nameof(storageFile));

            return new StorageFolderData(storageFile);
        }

        /// <inheritdoc/>
        public Task Delete()
        {
            return StorageFile.DeleteAsync().AsTask();
        }

        /// <inheritdoc />
        public async Task<Stream> GetStreamAsync(FileAccessMode accessMode = FileAccessMode.Read)
        {
            var stream = await StorageFile.OpenAsync((Windows.Storage.FileAccessMode)accessMode);

            return stream.AsStream();
        }

        /// <inheritdoc />
        public Task WriteAllBytesAsync(byte[] bytes)
        {
            return FileIO.WriteBytesAsync(StorageFile, bytes).AsTask();
        }

        /// <inheritdoc />
        public async Task<Stream> GetThumbnailAsync(ThumbnailMode thumbnailMode, uint requiredSize)
        {
            var thumbnail = await StorageFile.GetThumbnailAsync((Windows.Storage.FileProperties.ThumbnailMode)thumbnailMode, requiredSize);

            return thumbnail.AsStream();
        }

        public async Task<IMutableFileData> RenameAsync(string newFilename)
        {
            await StorageFile.RenameAsync(newFilename);
            return this;
        }

        public async Task<IMutableFileData> MoveAsync(IFolderData destination)
        {
            var dest = await GetStorageFolder(destination);
            await StorageFile.MoveAsync(dest, Name, NameCollisionOption.ReplaceExisting);
            return this;
        }

        public async Task<IMutableFileData> CopyAsync(IFolderData destination)
        {
            var dest = await GetStorageFolder(destination);
            await StorageFile.CopyAsync(dest, Name, NameCollisionOption.ReplaceExisting);
            return this;
        }

        public async Task<IMutableFileData> MoveAndRenameAsync(IFolderData destination, string newName)
        {
            var dest = await GetStorageFolder(destination);
            await StorageFile.MoveAsync(dest, newName, NameCollisionOption.ReplaceExisting);
            return this;
        }

        public async Task<IMutableFileData> CopyAndRenameAsync(IFolderData destination, string newName)
        {
            var dest = await GetStorageFolder(destination);
            await StorageFile.CopyAsync(dest, newName, NameCollisionOption.ReplaceExisting);
            return this;
        }

        private static async Task<StorageFolder> GetStorageFolder(IFolderData folder)
        {
            StorageFolder dest;
            if (folder is StorageFolderData stFldData)
                dest = stFldData.StorageFolder;
            else
                dest = await StorageFolder.GetFolderFromPathAsync(folder.Path);

            return dest;
        }
    }
}

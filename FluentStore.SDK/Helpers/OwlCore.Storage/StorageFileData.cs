// https://github.com/Arlodotexe/strix-music/blob/e7d2faee69420b6b4c75ece36aa2cbbedb34facb/src/Sdk/StrixMusic.Sdk.WinUI/Models/FileData.cs

using CommunityToolkit.Diagnostics;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Extensions;
using Windows.Storage;

namespace OwlCore.Storage.WinRT
{
    /// <inheritdoc cref="IAddressableFile"/>
    public class StorageFileData : IAddressableFile
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
        public async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var storageFile = await StorageFile.GetParentAsync();

            Guard.IsNotNull(storageFile, nameof(storageFile));

            return new StorageFolderData(storageFile);
        }

        /// <inheritdoc />
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = default)
        {
            var stream = await StorageFile.OpenAsync((Windows.Storage.FileAccessMode)accessMode);

            return stream.AsStream();
        }
    }
}

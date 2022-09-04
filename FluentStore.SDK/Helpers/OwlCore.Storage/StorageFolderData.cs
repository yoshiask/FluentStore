// https://github.com/Arlodotexe/strix-music/blob/e7d2faee69420b6b4c75ece36aa2cbbedb34facb/src/Sdk/StrixMusic.Sdk.WinUI/Models/FolderData.cs

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Extensions;
using Windows.Storage;

namespace OwlCore.Storage.WinRT
{
    /// <inheritdoc/>
    public class StorageFolderData : IAddressableFolder
    {
        /// <summary>
        /// The underlying <see cref="StorageFolder"/> instance in use.
        /// </summary>
        public StorageFolder StorageFolder { get; private set; }

        /// <summary>
        /// Constructs a new instance of <see cref="IAddressableFolder"/>.
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
        public async IAsyncEnumerable<IStorable> GetItemsAsync(StorableType type, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (type == StorableType.File)
            {
                var files = await StorageFolder.GetFilesAsync();

                foreach (var file in files)
                    yield return new StorageFileData(file);
            }
            else if (type == StorableType.Folder)
            {
                var folders = await StorageFolder.GetFoldersAsync();

                foreach (var folder in folders)
                    yield return new StorageFolderData(folder);
            }
            else if (type == StorableType.All)
            {
                var items = await StorageFolder.GetItemsAsync();

                foreach (var item in items)
                    if (item is StorageFile file)
                        yield return new StorageFileData(file);
                    else if (item is StorageFolder folder)
                        yield return new StorageFolderData(folder);
            }
        }

        /// <inheritdoc/>
        public async Task<IAddressableFolder> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var storageFolder = await StorageFolder.GetParentAsync();

            return new StorageFolderData(storageFolder);
        }
    }
}

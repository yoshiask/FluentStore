using OwlCore.Storage.SystemIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace OwlCore.Storage.Uwp
{
    /// <summary>
    /// An implementation of <see cref="IFolder"/> for <see cref="Windows.Storage.StorageFolder"/>.
    /// </summary>
    public class WindowsStorageFolder :
        IModifiableFolder,
        IChildFolder,
        IFastGetItem,
        IFastGetFirstByName,
        IFastGetItemRecursive,
        IFastFileMove<WindowsStorageFile>,
        IFastFileCopy<WindowsStorageFile>,
        IFastFileMove<SystemFile>,
        IFastFileCopy<SystemFile>
    {
        /// <summary>
        /// Creates a new instance of <see cref="WindowsStorageFolder"/>.
        /// </summary>
        public WindowsStorageFolder(IStorageFolder storageFolder)
        {
            StorageFolder = storageFolder;
        }

        /// <summary>
        /// The folder being wrapped.
        /// </summary>
        public IStorageFolder StorageFolder { get; }

        /// <summary>
        /// The system path to the folder.
        /// </summary>
        public string Path => StorageFolder.Path;

        /// <inheritdoc/>
        public string Id => Path;

        /// <inheritdoc/>
        public string Name => StorageFolder.Name;

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // We use paths as the ID. Extract the file name.
            var fileName = System.IO.Path.GetFileName(id);

            var item = await StorageFolder.GetItemAsync(fileName);

            cancellationToken.ThrowIfCancellationRequested();

            if (item is IStorageFile file)
                return new WindowsStorageFile(file);

            if (item is IStorageFolder folder)
                return new WindowsStorageFolder(folder);

            throw new FileNotFoundException();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (type == StorableType.None)
                throw new ArgumentOutOfRangeException(nameof(type), $"{nameof(StorableType)}.{type} is not valid here.");

            if (type.HasFlag(StorableType.All))
            {
                var items = await StorageFolder.GetItemsAsync();

                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (item is IStorageFolder folder)
                        yield return new WindowsStorageFolder(folder);

                    if (item is IStorageFile file)
                        yield return new WindowsStorageFile(file);
                }

                yield break;
            }

            if (type.HasFlag(StorableType.Folder))
            {
                var folders = await StorageFolder.GetFoldersAsync().AsTask(cancellationToken);

                foreach (var folder in folders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return new WindowsStorageFolder(folder);
                }
            }

            if (type.HasFlag(StorableType.File))
            {
                var files = await StorageFolder.GetFilesAsync().AsTask(cancellationToken);

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return new WindowsStorageFile(file);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
        {
            // We use the Path as the ID.
            if (!id.Contains(Id))
                throw new FileNotFoundException($"The item {id} was not found in the folder {Id}");

            // Will throw normally if we haven't been granted access to the file.
            // Windows.Storage doesn't seem to have a single method for retrieving a folder OR file from an arbitrary path
            // So we're forced to try / catch both options.

            // As a folder
            try
            {
                var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(id).AsTask(cancellationToken);
                return new WindowsStorageFolder(folder);
            }
            catch (FileNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            // As a file
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(id).AsTask(cancellationToken);
                return new WindowsStorageFile(file);
            }
            catch
            {
                throw new FileNotFoundException(@$"This folder ""{Id}"" could not retrieve expected descendent folder ""{id}"".");
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var item = await StorageFolder.GetItemAsync(name);

            return item switch
            {
                IStorageFile sf => new WindowsStorageFile(sf),
                IStorageFolder sfo => new WindowsStorageFolder(sfo),
                _ => throw new NotSupportedException($"{item.GetType()} not supported here."),
            };
        }

        /// <inheritdoc/>
        public async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (StorageFolder is StorageFolder folder)
                return new WindowsStorageFolder(await folder.GetParentAsync());

            throw new NotSupportedException($"{nameof(GetParentAsync)} is only supported when the provided {nameof(IStorageFile)} implementation also implements {nameof(IStorageItem2)}.");
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateCopyOfAsync(WindowsStorageFile fileToCopy, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!overwrite)
            {
                try
                {
                    var existingItem = await GetFirstByNameAsync(fileToCopy.Name, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    if (existingItem is IChildFile childFile)
                        return childFile;
                }
                catch (FileNotFoundException)
                {
                }
            }

            var storageFile = await fileToCopy.StorageFile.CopyAsync(StorageFolder, desiredNewName: fileToCopy.Name, option: overwrite ? NameCollisionOption.ReplaceExisting : NameCollisionOption.FailIfExists);
            cancellationToken.ThrowIfCancellationRequested();
            
            return new WindowsStorageFile(storageFile);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> MoveFromAsync(WindowsStorageFile fileToMove, IModifiableFolder source, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!overwrite)
            {
                try
                {
                    var existingItem = await GetFirstByNameAsync(fileToMove.Name, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    if (existingItem is IChildFile childFile)
                        return childFile;
                }
                catch (FileNotFoundException)
                {
                }
            }

            await fileToMove.StorageFile.MoveAsync(StorageFolder, fileToMove.Name, overwrite ? NameCollisionOption.ReplaceExisting : NameCollisionOption.FailIfExists);
            return fileToMove;
        }

        public async Task<IChildFile> MoveFromAsync(SystemFile fileToMove, IModifiableFolder source, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Will throw normally if we haven't been granted access to the file.
            var storageFile = await StorageFile.GetFileFromPathAsync(fileToMove.Path);
            var file = new WindowsStorageFile(storageFile);

            return await MoveFromAsync(file, source, overwrite, cancellationToken);
        }

        public async Task<IChildFile> CreateCopyOfAsync(SystemFile fileToCopy, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Will throw normally if we haven't been granted access to the file.
            var storageFile = await StorageFile.GetFileFromPathAsync(fileToCopy.Path);
            var file = new WindowsStorageFile(storageFile);

            return await CreateCopyOfAsync(file, overwrite, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var storageFile = await StorageFolder.CreateFileAsync(name, overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);

            return new WindowsStorageFile(storageFile);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var storageFolder = await StorageFolder.CreateFolderAsync(name, overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists);

            return new WindowsStorageFolder(storageFolder);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var parent = await item.GetParentAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (parent is null || parent.Id != Id)
                throw new FileNotFoundException("The provided item was not found in the folder");

            if (item is WindowsStorageFile file)
                await file.StorageFile.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask();

            if (item is WindowsStorageFolder folder)
                await folder.StorageFolder.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask();
        }

        /// <inheritdoc/>
        public virtual async Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (StorageFolder is IStorageFolderQueryOperations folder)
            {
                var knownItems = await folder.GetItemsAsync(startIndex: 0, maxItemsToRetrieve: uint.MaxValue);
                cancellationToken.ThrowIfCancellationRequested();
                return new WindowsStorageFolderWatcher(folder, knownItems);
            }

            throw new NotSupportedException($"{nameof(GetFolderWatcherAsync)} is only supported when the provided {nameof(IStorageFile)} implementation also implements {nameof(IStorageFolderQueryOperations)}.");
        }
    }
}

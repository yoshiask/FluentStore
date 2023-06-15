using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace OwlCore.Storage.Uwp
{
    /// <summary>
    /// An <see cref="IFolderWatcher"/> implementation for <see cref="WindowsStorageFolder"/>.
    /// </summary>
    public class WindowsStorageFolderWatcher : IFolderWatcher
    {
        private readonly SemaphoreSlim _changeMutex = new(1, 1);
        private readonly WindowsStorageFolder _folder;

        /// <summary>
        /// Creates a new instance of <see cref="WindowsStorageFolderWatcher"/>.
        /// </summary>
        /// <param name="folder">The folder being watched.</param>
        public WindowsStorageFolderWatcher(IStorageFolderQueryOperations folder, IReadOnlyList<IStorageItem> knownItems)
        {
            Tracker = folder.CreateItemQuery();
            _folder = new WindowsStorageFolder(Tracker.Folder);

            AttachEvents();
            KnownItems = knownItems;
        }

        private void AttachEvents()
        {
            Tracker.ContentsChanged += Tracker_ContentsChanged;
        }

        private void DetachEvents()
        {
            Tracker.ContentsChanged -= Tracker_ContentsChanged;
        }

        private async void Tracker_ContentsChanged(IStorageQueryResultBase sender, object args)
        {
            await _changeMutex.WaitAsync();

            try
            {
                // Only known reliable usage example found here -> https://github.com/ms-iot/securitysystem/blob/85ea8e97879f8b813ca1022aa4047791d4963643/SecuritySystemUWP/SecuritySystemUWP/AllJoynManager.cs#L53
                var files = await sender.Folder.GetItemsAsync();

                var addedItems = files.Except(KnownItems).ToList();
                var removedItems = KnownItems.Except(addedItems).ToList();

                if (addedItems.Count >= 1)
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems));

                if (removedItems.Count >= 1)
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));

                KnownItems = files;
            }
            finally
            {
                _changeMutex.Release();
            }
        }

        /// <inheritdoc/>
        public IMutableFolder Folder => _folder;

        /// <summary>
        /// The underlying tracker being used.
        /// </summary>
        public StorageItemQueryResult Tracker { get; }

        /// <summary>
        /// All known items present in the relevant folder.
        /// </summary>
        public IReadOnlyList<IStorageItem> KnownItems { get; private set; }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <inheritdoc/>
        public void Dispose()
        {
            DetachEvents();
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Dispose();
            return default;
        }
    }
}

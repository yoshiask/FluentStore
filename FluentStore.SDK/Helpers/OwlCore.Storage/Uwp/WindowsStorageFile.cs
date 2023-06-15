using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwlCore.Storage.Uwp
{
    /// <summary>
    /// An implementation of <see cref="IFile"/> for <see cref="Windows.Storage.StorageFile"/>.
    /// </summary>
    public class WindowsStorageFile : IFile, IChildFile
    {
        /// <summary>
        /// Creates a new instance of <see cref="WindowsStorageFile"/>.
        /// </summary>
        public WindowsStorageFile(IStorageFile storageFile)
        {
            StorageFile = storageFile;
        }

        /// <summary>
        /// The file being wrapped.
        /// </summary>
        public IStorageFile StorageFile { get; }

        /// <inheritdoc/>
        public string Id => Path;

        /// <inheritdoc/>
        public string Name => StorageFile.Name;

        /// <summary>
        /// The system path to the file.
        /// </summary>
        public string Path => StorageFile.Path;

        /// <inheritdoc/>
        public async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (StorageFile is IStorageItem2 file)
                return new WindowsStorageFolder(await file.GetParentAsync());

            throw new NotSupportedException($"{nameof(GetParentAsync)} is only supported when the provided {nameof(IStorageFile)} implementation also implements {nameof(IStorageItem2)}.");
        }

        /// <inheritdoc/>
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode = FileAccess.Read, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (accessMode.HasFlag(FileAccess.Write) && accessMode.HasFlag(FileAccess.Read))
            {
                var randomAccessStream = await StorageFile.OpenAsync(FileAccessMode.ReadWrite).AsTask(cancellationToken);
                return randomAccessStream.AsStream();
            }

            if (accessMode.HasFlag(FileAccess.Write))
                return await StorageFile.OpenStreamForWriteAsync();

            if (accessMode.HasFlag(FileAccess.Read))
                return await StorageFile.OpenStreamForReadAsync();

            throw new ArgumentOutOfRangeException(paramName: nameof(accessMode), message: $"{accessMode} cannot be used here.");
        }
    }
}

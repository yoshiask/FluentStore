using OwlCore.Storage;
using OwlCore.Storage.SystemIO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.SDK.Downloads
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Ensures the provided <see cref="IFile"/> is saved on disc.
        /// </summary>
        public static async Task<SystemFile> SaveLocally(this IFile file, SystemFolder folder, bool overwrite = false,
            CancellationToken cancellationToken = default)
        {
            if (file is SystemFile sysFile)
                return sysFile;

            return (SystemFile)await folder.CreateCopyOfAsync(file, overwrite, cancellationToken);
        }

        /// <summary>
        /// Ensures the provided <see cref="IFile"/> is saved on disc.
        /// Reports transfer progress if available.
        /// </summary>
        public static async Task<SystemFile> SaveLocally(this IFile file, SystemFolder folder, DataTransferProgress progress,
            bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (file is SystemFile sysFile)
            {
                progress.Report(1.0);
                return sysFile;
            }

            var dstFile = (SystemFile)await folder.CreateFileAsync(file.Name, overwrite, cancellationToken);
            return await SaveLocally(file, dstFile, progress, cancellationToken);
        }

        public static async Task<SystemFile> SaveLocally(this IFile srcFile, SystemFile dstFile, DataTransferProgress progress,
            CancellationToken cancellationToken = default)
        {
            if (srcFile is SystemFile sysFile)
            {
                progress.Report(1.0);
                return sysFile;
            }

            using var srcStream = await srcFile.OpenStreamAsync(cancellationToken: cancellationToken);
            using var dstStream = await dstFile.OpenStreamAsync(FileAccess.Write, cancellationToken: cancellationToken);

            await srcStream.CopyToAsync(dstStream, progress, cancellationToken: cancellationToken);

            return dstFile;
        }

        /// <summary>
        /// Asynchronously copies the source stream to the destination stream.
        /// </summary>
        /// <param name="source">The stream to copy from.</param>
        /// <param name="destination">The stream to copy to.</param>
        /// <param name="progress">The handler to use when progress is made.</param>
        /// <param name="bufferSize">The size of the internal intermediate byte array.</param>
        /// https://stackoverflow.com/questions/39742515/stream-copytoasync-with-progress-reporting-progress-is-reported-even-after-cop
        public static async Task CopyToAsync(this Stream source, Stream destination, DataTransferProgress progress, int bufferSize = 0x1000, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;

            try
            {
                // Attempt to get total number of bytes
                progress.TotalBytes = source.Length;
            }
            catch { }

            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                progress.Report(bytesRead);
            }

            await destination.FlushAsync(cancellationToken);
        }
    }
}

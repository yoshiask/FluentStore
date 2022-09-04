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
        /// Ensures the provided <paramref name="file"/> is saved on disc.
        /// </summary>
        public static async Task<SystemFile> SaveLocally(this IFile file, SystemFolder folder, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            if (file is SystemFile sysFile)
                return sysFile;

            return (SystemFile)await folder.CreateCopyOfAsync(file, overwrite, cancellationToken);
        }

        /// <summary>
        /// Asynchronously copies the source stream to the destination stream.
        /// </summary>
        /// <param name="source">The stream to copy from.</param>
        /// <param name="destination">The stream to copy to.</param>
        /// <param name="progress">The handler to use when progress is made.</param>
        /// <param name="bufferSize">The size of the internal intermediate byte array.</param>
        /// https://stackoverflow.com/questions/39742515/stream-copytoasync-with-progress-reporting-progress-is-reported-even-after-cop
        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<double?> progress, int bufferSize = 0x1000, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[bufferSize];
            int bytesRead;
            long totalRead = 0;

            long total = -1;
            try
            {
                // Attempt to get total number of bytes
                total = source.Length;
            }
            catch { }

            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                totalRead += bytesRead;

                double? percent = total >= 0 ? totalRead / total : null;
                progress.Report(percent);
            }
        }
    }
}

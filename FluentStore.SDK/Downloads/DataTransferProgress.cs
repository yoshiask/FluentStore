using System;

namespace FluentStore.SDK.Downloads
{
    public class DataTransferProgress : IProgress<double?>
    {
        /// <summary>The handler specified to the constructor.  This may be null.</summary>
        private readonly Action<DataTransferProgress>? _handler;

        /// <summary>Initializes the <see cref="DataTransferProgress"/>.</summary>
        public DataTransferProgress()
        {
        }

        /// <summary>Initializes the <see cref="DataTransferProgress"/> with the specified callback.</summary>
        /// <param name="handler">
        /// A handler to invoke for each reported progress value.  This handler will be invoked
        /// in addition to any delegates registered with the <see cref="ProgressChanged"/> event.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is <see langword="null"/>.</exception>
        public DataTransferProgress(Action<DataTransferProgress> handler) : this()
        {
            ArgumentNullException.ThrowIfNull(handler);

            _handler = handler;
        }

        /// <summary>Raised for each reported progress value.</summary>
        public event Action<DataTransferProgress>? ProgressChanged;

        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; } = -1;
        public double? PercentComplete { get; set; }

        public void Report(double? value)
        {
            PercentComplete = value;

            _handler?.Invoke(this);
            ProgressChanged?.Invoke(this);
        }

        public void Report(int newBytesDownloaded, long totalBytes = -1)
        {
            if (totalBytes >= 0)
                TotalBytes = totalBytes;

            BytesDownloaded += newBytesDownloaded;

            double? percent = TotalBytes >= 0 ? BytesDownloaded / TotalBytes : null;
            Report(percent);
        }
    }
}

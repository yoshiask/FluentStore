namespace FluentStore.SDK.Downloads
{
    public record DataTransferProgress
    {
        public long BytesDownloaded;
        public long TotalBytes;
        public double? PercentComplete;
    }
}

using System;

namespace FluentStore.SDK.Messages
{
    public class PackageDownloadFailedMessage : ErrorMessage<PackageBase>
    {
        public PackageDownloadFailedMessage(Exception ex, PackageBase context = null) : base(ex, context)
        {
        }
    }
}

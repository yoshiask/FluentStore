using System;

namespace FluentStore.SDK.Messages
{
    public class PackageInstallFailedMessage : ErrorMessage<PackageBase>
    {
        public PackageInstallFailedMessage(Exception ex, PackageBase context = null) : base(ex, context)
        {
        }
    }
}

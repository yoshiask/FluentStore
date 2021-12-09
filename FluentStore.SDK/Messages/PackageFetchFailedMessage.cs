using System;

namespace FluentStore.SDK.Messages
{
    public class PackageFetchFailedMessage : ErrorMessage<PackageBase>
    {
        public PackageFetchFailedMessage(Exception ex, PackageBase context = null) : base(ex, context)
        {
        }
    }
}

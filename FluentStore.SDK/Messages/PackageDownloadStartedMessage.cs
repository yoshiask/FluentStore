using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FluentStore.SDK.Messages
{
    public class PackageDownloadStartedMessage : ValueChangedMessage<PackageBase>
    {
        public PackageDownloadStartedMessage(PackageBase package) : base(package)
        {

        }

        public PackageBase Package => Value;
    }
}

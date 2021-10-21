using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FluentStore.SDK.Messages
{
    public class PackageInstallStartedMessage : ValueChangedMessage<PackageBase>
    {
        public PackageInstallStartedMessage(PackageBase package) : base(package)
        {

        }

        public PackageBase Package => Value;
    }
}

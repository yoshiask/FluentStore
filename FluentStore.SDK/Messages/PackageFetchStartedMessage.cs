using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FluentStore.SDK.Messages
{
    public class PackageFetchStartedMessage : ValueChangedMessage<PackageBase>
    {
        public PackageFetchStartedMessage(PackageBase package) : base(package)
        {

        }

        public PackageBase Package => Value;
    }
}

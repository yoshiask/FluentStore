using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FluentStore.SDK.Messages
{
    public class PackageFetchCompletedMessage : ValueChangedMessage<PackageBase>
    {
        public PackageFetchCompletedMessage(PackageBase package) : base(package)
        {

        }

        public PackageBase Package => Value;
    }
}

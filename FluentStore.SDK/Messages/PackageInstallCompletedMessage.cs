using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace FluentStore.SDK.Messages
{
    public class PackageInstallCompletedMessage : ValueChangedMessage<PackageBase>
    {

        public PackageInstallCompletedMessage(PackageBase package) : base(package)
        {

        }

        public PackageBase Package => Value;
    }
}

using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{

    public class PackageInstallFailedMessage : ValueChangedMessage<Tuple<PackageBase, Exception>>
    {
        public PackageInstallFailedMessage(Tuple<PackageBase, Exception> info) : base(info)
        {

        }

        public PackageInstallFailedMessage(PackageBase package, Exception ex)
            : base(new Tuple<PackageBase, Exception>(package, ex))
        {

        }

        public PackageBase Package => Value.Item1;
        public Exception Exception => Value.Item2;
    }
}

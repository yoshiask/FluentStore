using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{
    public class PackageFetchFailedMessage : ValueChangedMessage<Tuple<PackageBase, Exception>>
    {
        public PackageFetchFailedMessage(Tuple<PackageBase, Exception> info) : base(info)
        {

        }

        public PackageFetchFailedMessage(PackageBase package, Exception ex)
            : base(new Tuple<PackageBase, Exception>(package, ex))
        {

        }

        public PackageBase Package => Value.Item1;
        public Exception Exception => Value.Item2;
    }
}

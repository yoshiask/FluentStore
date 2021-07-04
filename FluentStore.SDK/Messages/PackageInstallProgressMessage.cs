using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{
    public class PackageInstallProgressMessage : ValueChangedMessage<Tuple<PackageBase, double>>
    {
        public PackageInstallProgressMessage(Tuple<PackageBase, double> info) : base(info)
        {

        }

        public PackageInstallProgressMessage(PackageBase package, double progress)
            : base(new Tuple<PackageBase, double>(package, progress))
        {

        }

        public PackageBase Package => Value.Item1;
        public double Progress => Value.Item2;
    }
}

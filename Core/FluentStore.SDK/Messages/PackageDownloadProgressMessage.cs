using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{
    public class PackageDownloadProgressMessage : ValueChangedMessage<Tuple<PackageBase, double, double>>
    {
        public PackageDownloadProgressMessage(Tuple<PackageBase, double, double> info) : base(info)
        {

        }

        public PackageDownloadProgressMessage(PackageBase package, double downloaded, double total)
            : base(new Tuple<PackageBase, double, double>(package, downloaded, total))
        {

        }

        public PackageBase Package => Value.Item1;
        public double Downloaded => Value.Item2;
        public double Total => Value.Item3;
    }
}

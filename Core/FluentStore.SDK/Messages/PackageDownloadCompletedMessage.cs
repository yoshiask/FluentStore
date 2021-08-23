using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;
using Windows.Storage;

namespace FluentStore.SDK.Messages
{
    public class PackageDownloadCompletedMessage : ValueChangedMessage<Tuple<PackageBase, StorageFile>>
    {
        public PackageDownloadCompletedMessage(Tuple<PackageBase, StorageFile> info) : base(info)
        {

        }

        public PackageDownloadCompletedMessage(PackageBase package, StorageFile installerFile)
            : base(new Tuple<PackageBase, StorageFile>(package, installerFile))
        {

        }

        public PackageBase Package => Value.Item1;
        public StorageFile InstallerFile => Value.Item2;
    }
}

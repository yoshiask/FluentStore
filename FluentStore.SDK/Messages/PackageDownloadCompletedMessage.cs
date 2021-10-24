using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.IO;
using Windows.Storage;

namespace FluentStore.SDK.Messages
{
    public class PackageDownloadCompletedMessage : ValueChangedMessage<Tuple<PackageBase, FileInfo>>
    {
        public PackageDownloadCompletedMessage(Tuple<PackageBase, FileInfo> info) : base(info)
        {

        }

        public PackageDownloadCompletedMessage(PackageBase package, FileInfo installerFile)
            : base(new Tuple<PackageBase, FileInfo>(package, installerFile))
        {

        }

        public PackageBase Package => Value.Item1;
        public FileInfo InstallerFile => Value.Item2;
    }
}

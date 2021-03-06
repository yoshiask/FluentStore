﻿using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{

    public class PackageDownloadFailedMessage : ValueChangedMessage<Tuple<PackageBase, Exception>>
    {
        public PackageDownloadFailedMessage(Tuple<PackageBase, Exception> info) : base(info)
        {

        }

        public PackageDownloadFailedMessage(PackageBase package, Exception ex)
            : base(new Tuple<PackageBase, Exception>(package, ex))
        {

        }

        public PackageBase Package => Value.Item1;
        public Exception Exception => Value.Item2;
    }
}

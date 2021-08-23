using System;
using System.Collections.Generic;

namespace FluentStore.SDK.Models
{
    public class HandlerPackagePair : Tuple<PackageHandlerBase, PackageBase>
    {
        public HandlerPackagePair(PackageHandlerBase handler, PackageBase package) : base(handler, package)
        {

        }

        public PackageHandlerBase PackageHandler => Item1;
        public PackageBase Package => Item2;
    }
    public class HandlerPackageListPair : Tuple<PackageHandlerBase, IEnumerable<PackageBase>>
    {
        public HandlerPackageListPair(PackageHandlerBase handler, IEnumerable<PackageBase> packages) : base(handler, packages)
        {

        }

        public PackageHandlerBase PackageHandler => Item1;
        public IEnumerable<PackageBase> Packages => Item2;
    }
}

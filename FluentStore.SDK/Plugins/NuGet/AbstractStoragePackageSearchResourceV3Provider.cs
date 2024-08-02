using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

#pragma warning disable CS0618 // Type or member is obsolete

namespace FluentStore.SDK.Plugins.NuGet;

public class AbstractStoragePackageSearchResourceV3Provider : ResourceProvider
{
    public AbstractStoragePackageSearchResourceV3Provider() : base(typeof(PackageSearchResource),
        nameof(AbstractStoragePackageSearchResourceV3Provider), nameof(PackageSearchResourceV2FeedProvider))
    {
    }

    public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
    {
        PackageSearchResource curResource = null;
        var serviceIndex = await source.GetResourceAsync<ServiceIndexResourceV3>(token);

        if (serviceIndex != null)
        {
            var endpoints = serviceIndex.GetServiceEntryUris(ServiceTypes.SearchQueryService);

            curResource = new AbstractStoragePackageSearchResourceV3(endpoints);
        }

        return new Tuple<bool, INuGetResource>(curResource != null, curResource);
    }
}
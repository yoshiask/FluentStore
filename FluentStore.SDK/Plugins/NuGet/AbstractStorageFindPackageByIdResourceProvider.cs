using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using OwlCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Extensions;

namespace FluentStore.SDK.Plugins.NuGet;

// See NuGet.Protocol.HttpFileSystemBasedFindPackageByIdResourceProvider

public class AbstractStorageFindPackageByIdResourceProvider : ResourceProvider
{
    public AbstractStorageFindPackageByIdResourceProvider() : base(typeof(FindPackageByIdResource),
        nameof(AbstractStorageFindPackageByIdResourceProvider), nameof(RemoteV3FindPackageByIdResourceProvider))
    {
    }

    public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
    {
        INuGetResource resource = null;
        IReadOnlyList<Uri> packageBaseAddress = (await source.GetResourceAsync<ServiceIndexResourceV3>(token))?
            .GetServiceEntryUris(ServiceTypes.PackageBaseAddress)
            ?? source.PackageSource.TrySourceAsUri?.IntoList();

        if (packageBaseAddress is { Count: > 0 })
        {
            (await source.GetResourceAsync<RepositorySignatureResource>(token))?.UpdateRepositorySignatureInfo();
            var sourceResource = await source.GetResourceAsync<AbstractStorageResource>(token);

            resource = new AbstractStorageFindPackageByIdResource(packageBaseAddress, sourceResource.File);
        }

        return Tuple.Create(resource is not null, resource);
    }
}



public class AbstractStorageResource : INuGetResource
{
    public IFile File { get; }

    public AbstractStorageResource(IFile file)
    {
        File = file;
    }
}

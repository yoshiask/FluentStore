// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentStore.SDK.Downloads;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace FluentStore.SDK.Plugins.NuGet;

public class AbstractStorageResourceProvider : ResourceProvider
{
    // Only one AbstractStorage per source should exist. This is to reduce the number of TCP connections.
    private readonly ConcurrentDictionary<PackageSource, AbstractStorageResource> _cache = new();

    /// <summary>
    /// The throttle to apply to all <see cref="AbstractStorage"/> requests.
    /// </summary>
    public static IThrottle Throttle { get; set; }

    public AbstractStorageResourceProvider()
        : base(typeof(AbstractStorageResource),
              nameof(AbstractStorageResource),
              NuGetResourceProviderPositions.Last)
    {
    }

    public override Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
    {
        Debug.Assert(source.PackageSource.SourceUri != null, "OwlCore.Storage source requested without specifying a URI.");

        AbstractStorageResource curResource = null;

        IThrottle throttle = NullThrottle.Instance;

        if (Throttle != null)
        {
            throttle = Throttle;
        }
        else if (source.PackageSource.MaxHttpRequestsPerSource > 0)
        {
            throttle = SemaphoreSlimThrottle.CreateSemaphoreThrottle(source.PackageSource.MaxHttpRequestsPerSource);
        }

        curResource = _cache.GetOrAdd(
            source.PackageSource,
            packageSource => new AbstractStorageResource(AbstractStorageHelper.GetFileFromUrl(packageSource.SourceUri.ToString())));

        return Task.FromResult(new Tuple<bool, INuGetResource>(curResource != null, curResource));
    }
}
using System;
using FluentStore.SDK.Models;
using Microsoft.Msix.Utils.AppxPackagingInterop;
using StoreDownloader;

namespace FluentStore.Sources.Microsoft.Store;

internal record AppxPackageDependency(string Name, Version MinVersion, Architecture Architecture)
{
    public AppxPackageDependency(IAppxManifestPackageDependency appxDependency, Architecture arch)
        : this(appxDependency.GetName(), MSStoreDownloader.GetVersionFromULong(appxDependency.GetMinVersion()), arch)
    {
    }

    public bool IsFullfilledBy(PackageFullName pfn)
    {
        return pfn.Name == Name
            && pfn.Version >= MinVersion
            && pfn.Architecture == Architecture;
    }

    public override string ToString() => $"{Name}_{MinVersion}_{Architecture}";
}
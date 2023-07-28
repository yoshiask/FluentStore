﻿using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Versioning;
using System;

namespace FluentStore.SDK.Plugins;

internal record struct PluginEntry(string Id, NuGetVersion Version, NuGetFramework Framework, PluginInstallStatus Status)
{
    public PluginEntry(PackageIdentity packageIdentity, NuGetFramework framework, PluginInstallStatus status)
        : this(packageIdentity.Id, packageIdentity.Version, framework, status) { }

    public static PluginEntry Parse(string text)
    {
        var cells = text.Split(',', StringSplitOptions.TrimEntries);
        return new(cells[0], NuGetVersion.Parse(cells[1]), NuGetFramework.Parse(cells[2]), Enum.Parse<PluginInstallStatus>(cells[^1]));
    }

    public PackageReference ToPackageReference() => new(GetPackageIdentity(), Framework);

    public PackageIdentity GetPackageIdentity() => new(Id, Version);

    public override string ToString() => string.Join(',', Id, Version, Framework, Status);
}
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Versioning;
using System;

namespace FluentStore.SDK.Plugins;

public record struct PluginEntry(string Id, NuGetVersion Version, NuGetFramework Framework, PluginInstallStatus InstallStatus, PluginInstallStatus UninstallStatus)
{
    public PluginEntry(PackageIdentity packageIdentity, NuGetFramework framework, PluginInstallStatus installStatus, PluginInstallStatus uninstallStatus)
        : this(packageIdentity.Id, packageIdentity.Version, framework, installStatus, uninstallStatus) { }

    public static PluginEntry Parse(string text)
    {
        var cells = text.Split('\t', StringSplitOptions.TrimEntries);

        if (cells.Length < 5 || !Enum.TryParse(cells[4], out PluginInstallStatus uninstallStatus))
            uninstallStatus = PluginInstallStatus.NoAction;

        return new(cells[0],
            NuGetVersion.Parse(cells[1]), NuGetFramework.Parse(cells[2]),
            Enum.Parse<PluginInstallStatus>(cells[3]),
            uninstallStatus);
    }

    public readonly PackageReference ToPackageReference() => new(ToPackageIdentity(), Framework);

    public readonly PackageIdentity ToPackageIdentity() => new(Id, Version);

    public override readonly string ToString() => string.Join('\t', Id, Version, Framework, InstallStatus, UninstallStatus);
}

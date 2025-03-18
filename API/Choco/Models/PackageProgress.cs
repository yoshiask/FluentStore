using NuGet.Versioning;

namespace Chocolatey.Models;

public class PackageProgress(string id, NuGetVersion version, int percentage)
{
    public string Id { get; } = id;

    public NuGetVersion Version { get; } = version;

    public int Percentage { get; } = percentage;

    public override string ToString() => $"{Id} {Version} {Percentage:##}%";
}

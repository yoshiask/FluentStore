using System;
using System.Text.RegularExpressions;

namespace FluentStore.SDK.Models;

public partial record PackageFamilyName(string Name, string PublisherId)
    : IEquatable<PackageFamilyName>, IEquatable<PackageFullName>, IEquatable<PackageIdentity>
{
    public override string ToString() => $"{Name}_{PublisherId}";

    public static PackageFamilyName Parse(string packageFullName)
    {
        var rx = PackageFamilyNameRegex();
        var match = rx.Match(packageFullName);
        if (!match.Success)
            throw new FormatException();

        var name = match.Groups["name"].Value;
        var publisherId = match.Groups["pub"].Value;

        return new(name, publisherId);
    }

    [GeneratedRegex($"^{PackageFullName.RxName}_{PackageFullName.RxPublisherId}$", RegexOptions.IgnoreCase)]
    public static partial Regex PackageFamilyNameRegex();

    public bool Equals(PackageFullName other) => Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase)
        && PublisherId.Equals(other.PublisherId, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => HashCode.Combine(Name.ToUpperInvariant(), PublisherId.ToUpperInvariant());

    public bool Equals(PackageIdentity other) => other?.Equals(this) ?? false;
}

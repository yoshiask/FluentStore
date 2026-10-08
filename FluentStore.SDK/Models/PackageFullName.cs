using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FluentStore.SDK.Models;

public sealed partial record PackageFullName(string Name, Version Version, Architecture Architecture, string ResourceId, string PublisherId)
    : IEquatable<PackageFullName>, IEquatable<PackageFamilyName>
{
    public override string ToString() => $"{Name}_{Version}_{Architecture}_{ResourceId}_{PublisherId}";

    public PackageFamilyName ToPackageFamilyName() => new(Name, PublisherId);

    public static bool TryParse(string packageFullName, [NotNullWhen(true)] out PackageFullName pfn)
    {
        var rx = PackageFullNameRegex();
        var match = rx.Match(packageFullName);
        if (!match.Success)
        {
            pfn = null;
            return false;
        }

        var name = match.Groups["name"].Value;
        var version = Version.Parse(match.Groups["ver"].Value);
        var architecture = Enum.Parse<Architecture>(match.Groups["arch"].Value, true);
        var resourceId = match.Groups["rsrc"].Value;
        var publisherId = match.Groups["pub"].Value;

        pfn = new(name, version, architecture, resourceId, publisherId);
        return true;
    }

    public static PackageFullName Parse(string packageFullName)
    {
        if (!TryParse(packageFullName, out var pfn))
            throw new FormatException();
        return pfn;
    }

    [GeneratedRegex($"^{RxName}_{RxVersion}_{RxArchitecture}_{RxResourceId}_{RxPublisherId}$", RegexOptions.IgnoreCase)]
    public static partial Regex PackageFullNameRegex();

    [StringSyntax("regex")]
    public const string RxName = @"(?<name>[A-Za-z\d.-]{3,50})";

    [StringSyntax("regex")]
    public const string RxVersion = @"(?<ver>[\d.]+)";

    [StringSyntax("regex")]
    public const string RxArchitecture = @"(?<arch>[^_]+)";

    [StringSyntax("regex")]
    public const string RxResourceId = @"(?<rsrc>[A-Za-z\d.-]{0,30}|~)";

    [StringSyntax("regex")]
    public const string RxPublisherId = @"(?<pub>[a-hjkmnp-tv-z0-9]{13}?)";

    public bool Equals(PackageFullName other)
    {
        if (other is null)
            return false;

        return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(PackageFamilyName other) => other?.Equals(this) ?? false;

    public override int GetHashCode()
    {
        return HashCode.Combine(Name.ToUpperInvariant(), Version, Architecture,
            ResourceId.ToUpperInvariant(), PublisherId.ToUpperInvariant());
    }
}

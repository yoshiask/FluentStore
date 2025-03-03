using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FluentStore.SDK.Models;

public partial record PackageFullName(string Name, Version Version, Architecture Architecture, string ResourceId, string PublisherId)
    : IEquatable<PackageFullName>
{
    public override string ToString() => $"{Name}_{Version}_{Architecture}_{ResourceId}_{PublisherId}";

    public PackageFamilyName ToPackageFamilyName() => new(Name, PublisherId);

    public static PackageFullName Parse(string packageFullName)
    {
        var rx = PackageFullNameRegex();
        var match = rx.Match(packageFullName);
        if (!match.Success)
            throw new FormatException();

        var name = match.Groups["name"].Value;
        var version = Version.Parse(match.Groups["ver"].Value);
        var architecture = Enum.Parse<Architecture>(match.Groups["arch"].Value, true);
        var resourceId = match.Groups["rsrc"].Value;
        var publisherId = match.Groups["pub"].Value;

        return new(name, version, architecture, resourceId, publisherId);
    }

    [GeneratedRegex($"^{RxName}_{RxVersion}_{RxArchitecture}_{RxResourceId}_{RxPublisherId}$")]
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
}

public partial record PackageFamilyName(string Name, string PublisherId)
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

    [GeneratedRegex($"^{PackageFullName.RxName}_{PackageFullName.RxPublisherId}$")]
    public static partial Regex PackageFamilyNameRegex();
}

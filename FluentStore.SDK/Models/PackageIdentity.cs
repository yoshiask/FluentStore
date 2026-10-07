using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FluentStore.SDK.Models;

public sealed record PackageIdentity(string Name, Version Version, Architecture Architecture, string Publisher)
    : IEquatable<PackageIdentity>, IEquatable<PackageFamilyName>
{
    public string PublisherId { get; } = GetPublisherId(Publisher);

    public PackageFamilyName ToPackageFamilyName() => new(Name, PublisherId);

    public PackageFullName ToPackageFullName(string resourceId) => new(Name, Version, Architecture, resourceId, PublisherId);

    public override string ToString() => $"{Name}_{Version}_{Architecture}_{PublisherId}";

    public bool Equals(PackageFamilyName other)
    {
        if (other is null)
            return false;

        return Name.Equals(other.Name, StringComparison.Ordinal)
            && PublisherId.Equals(other.PublisherId, StringComparison.Ordinal);
    }

    private static string GetPublisherId(string publisher)
    {
        // https://gist.github.com/marcinotorowski/6a51023600160fcceef9ceea341bbc4a
        // https://marcinotorowski.com/2021/12/19/calculating-hash-part-of-msix-package-family-name/

        var encoded = SHA256.HashData(Encoding.Unicode.GetBytes(publisher));
        var binaryString = string.Concat(encoded.Take(8).Select(c => Convert.ToString(c, 2).PadLeft(8, '0'))) + '0'; // representing 65-bits = 13 * 5
        var encodedPublisherId = string.Concat(Enumerable.Range(0, binaryString.Length / 5).Select(i => "0123456789abcdefghjkmnpqrstvwxyz".Substring(Convert.ToInt32(binaryString.Substring(i * 5, 5), 2), 1)));
        return encodedPublisherId;
    }
}

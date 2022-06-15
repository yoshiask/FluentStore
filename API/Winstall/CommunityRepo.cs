using Flurl;
using Flurl.Http;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Winstall.Models.Manifest;
using Winstall.Models.Manifest.Enums;
using YamlDotNet.Serialization;

namespace Winstall;

public static class CommunityRepo
{
    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithTypeConverter(new YamlStringEnumConverter()).Build();

    public static Task<VersionManifest> GetManifestAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        return GetAndDeserializeAsync<VersionManifest>(id, version, cancellationToken: cancellationToken);
    }

    public static Task<InstallerManifest> GetInstallerAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        return GetAndDeserializeAsync<InstallerManifest>(id, version, "installer", cancellationToken);
    }

    public static Task<Locale> GetLocaleAsync(string id, string version, string locale, CancellationToken cancellationToken = default)
    {
        return GetAndDeserializeAsync<Locale>(id, version, $"locale.{locale}", cancellationToken);
    }

    public static Task<Locale> GetLocaleAsync(string id, string version, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        return GetLocaleAsync(id, version, culture.ToString(), cancellationToken);
    }

    public static async Task<DefaultLocale> GetDefaultLocaleAsync(string id, string version, CancellationToken cancellationToken = default)
    {
        var manifest = await GetManifestAsync(id, version, cancellationToken);
        return await GetAndDeserializeAsync<DefaultLocale>(id, version,
            $"locale.{manifest.DefaultLocale}", cancellationToken);
    }

    private static async Task<TManifest> GetAndDeserializeAsync<TManifest>(string id, string version, string manifestType = null, CancellationToken cancellationToken = default)
    {
        var url = BuildManifestUrl(id, version, manifestType);
        var yaml = await url.GetStringAsync(cancellationToken);

        return _deserializer.Deserialize<TManifest>(yaml);
    }

    private static Url BuildManifestUrl(string id, string version, string manifestType = null)
    {
        string filename = manifestType == null
            ? $"{id}.yaml"
            : $"{id}.{manifestType}.yaml";

        return "https://raw.githubusercontent.com/microsoft/winget-pkgs/master/manifests"
            .AppendPathSegments(char.ToLower(id[0]))
            .AppendPathSegments(id.Split('.'))
            .AppendPathSegments(version, filename);
    }
}

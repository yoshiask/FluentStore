using FluentStore.SDK;
using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Models;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Ipfs;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FluentStore.Helpers.Updater;

internal class AppUpdatePackageSource() : PackageHandlerBase(null)
{
    //private const string LATEST_JSON_PATH = "file://E:\\Documents\\site\\ipfs\\FluentStore\\versions.json";
    private const string LATEST_JSON_PATH = "ipns://ipfs.askharoun.com/FluentStore/versions.json";

    private Dictionary<string, IOrderedEnumerable<OnlineVersionInfo>> _index;

    public const string NAMESPACE_FLUENTSTORE_DU = "fluentstore-du";

    public override HashSet<string> HandledNamespaces { get; } = [NAMESPACE_FLUENTSTORE_DU];

    public override string DisplayName => "Fluent Store Distributed Updater";

    public override ImageBase GetImage() => new FileImage("ms-appx:///Assets/AppIcon.ico");

    public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus targetStatus = PackageStatus.Details)
    {
        var release = packageUrn.GetContent<RawNamespaceSpecificString>().UnEscapedValue;
        var index = await GetIndexAsync();

        if (!index.TryGetValue(release, out var versionInfos))
            return null;

        var arch = Win32Helper.GetSystemArchitecture();
        var info = versionInfos
            .FirstOrDefault(i => i.Installers.TryGetValue(arch, out var installer) && installer.IsValid());

        return new AppUpdatePackage(this, info, release, info.Installers[arch]);
    }

    public override Task<PackageBase> GetPackageFromUrl(Url url) => throw new NotImplementedException();

    public override Url GetUrlFromPackage(PackageBase package) => throw new NotImplementedException();

    public static Urn FormatUrn(string release) => Urn.Parse($"urn:{NAMESPACE_FLUENTSTORE_DU}:{release}");

    private async Task<Dictionary<string, IOrderedEnumerable<OnlineVersionInfo>>> GetIndexAsync()
    {
        if (_index is null)
        {
            // Fetch the most up-to-date version information
            var latestJsonFile = AbstractStorageHelper.GetFileFromUrl(LATEST_JSON_PATH);
            using var stream = await latestJsonFile.SafeOpenStreamAsync(System.IO.FileAccess.Read);

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            var versions = await JsonSerializer.DeserializeAsync<List<OnlineVersionInfo>>(stream, options);

            _index = versions
                .GroupBy(x => x.Version.Release)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(v => v.Version));
        }

        return _index;
    }
}

internal class OnlineVersionInfo
{
    [JsonPropertyName("version")]
    public string VersionStr { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("installers")]
    public Dictionary<Architecture, OnlineInstallerInfo> Installers { get; set; }

    [JsonIgnore]
    public NuGetVersion Version => NuGetVersion.Parse(VersionStr);
}

internal class OnlineInstallerInfo
{
    [JsonPropertyName("type")]
    public InstallerType Type { get; set; }

    [JsonPropertyName("cid")]
    public string CidStr { get; set; }

    [JsonPropertyName("httpUrl")]
    public string HttpUrl { get; set; }

    [JsonIgnore]
    public Cid Cid => Cid.Decode(CidStr);

    public bool IsValid() => Cid is not null || HttpUrl is not null;
}

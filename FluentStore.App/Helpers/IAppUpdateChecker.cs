using FluentStore.SDK.Downloads;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Models;
using Ipfs;
using NuGet.Versioning;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.Helpers;

public interface IAppUpdateService
{
    Task<UpdateAvailableInfo> FetchAvailableUpdate(CancellationToken token = default);

    Task DownloadUpdate(UpdateAvailableInfo update, string destinationDirectory, IProgress<double> progress, CancellationToken token = default);
}

public record UpdateAvailableInfo(string Name, NuGetVersion Version, DateTimeOffset? ReleasedAt);

public class LatestJsonUpdateService(string latestJsonPath = LatestJsonUpdateService.DEFAULT_LATEST_JSON_PATH) : IAppUpdateService
{
    private const string DEFAULT_LATEST_JSON_PATH = "ipns://ipfs.askharoun.com/FluentStore/Latest/latest.json";

    public async Task<UpdateAvailableInfo> FetchAvailableUpdate(CancellationToken token = default)
    {
        try
        {
            // Fetch the latest version information
            var latestJsonFile = AbstractStorageHelper.GetFileFromUrl(latestJsonPath);

            token.ThrowIfCancellationRequested();

            List<OnlineVersionInfo> versions = null;

            using (var stream = await latestJsonFile.SafeOpenStreamAsync(System.IO.FileAccess.Read, token))
            {
                versions = await JsonSerializer.DeserializeAsync<List<OnlineVersionInfo>>(stream, cancellationToken: token);
            }

            var architecture = Win32Helper.GetSystemArchitecture();
            var appVersion = SDK.Plugins.NuGet.FluentStoreNuGetProject.CurrentSdkVersion;

            var latestSupported = versions
                .OrderByDescending(x => x.Version)
                .Where(x => x.Installers.TryGetValue(architecture, out var i) && i.IsValid())
                .FirstOrDefault();

            if (appVersion < latestSupported.Version)
                return new OnlineUpdateAvailableInfo(latestSupported, architecture);
        }
        catch { }

        return null;
    }

    public async Task DownloadUpdate(UpdateAvailableInfo update, string destinationDirectory, IProgress<double> progress, CancellationToken token = default)
    {
        if (update is not OnlineUpdateAvailableInfo onlineUpdate)
            throw new ArgumentException("", nameof(update));

        IFile srcFile = onlineUpdate.Installer switch
        {
            { Cid: not null } => new IpfsFile(onlineUpdate.Installer.Cid, AbstractStorageHelper.IpfsClient),
            { HttpUrl: not null } => AbstractStorageHelper.GetFileFromUrl(onlineUpdate.Installer.HttpUrl),

            _ => throw new InvalidProgramException($"Update {update.Name} does not specify any supported files to download.")
        };

        progress?.Report(0.0);

        SystemFolder dstDir = new(destinationDirectory);
        await dstDir.CreateCopyOfAsync(srcFile, true, token);

        progress?.Report(1.0);
    }

    private class OnlineVersionInfo
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

    private class OnlineInstallerInfo
    {
        [JsonPropertyName("cid")]
        public string CidStr { get; set; }

        [JsonPropertyName("httpUrl")]
        public string HttpUrl { get; set; }

        [JsonIgnore]
        public Cid Cid => Cid.Decode(CidStr);

        public bool IsValid() => Cid is not null || HttpUrl is not null;
    }

    private record OnlineUpdateAvailableInfo : UpdateAvailableInfo
    {
        public OnlineUpdateAvailableInfo(OnlineVersionInfo info, Architecture arch)
            : base($"{info.Version} {arch}", info.Version, null)
        {
            Installer = info.Installers[arch];
        }

        public OnlineInstallerInfo Installer { get; }
    }
}

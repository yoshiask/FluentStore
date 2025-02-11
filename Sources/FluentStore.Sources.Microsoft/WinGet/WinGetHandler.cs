using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.SDK.Helpers;
using System;
using OwlCore.ComponentModel;
using System.Threading;

namespace FluentStore.Sources.Microsoft.WinGet
{
    public partial class WinGetProxyHandler : PackageHandlerBase, IAsyncInit
    {
        public const string NAMESPACE_WINGET = "winget";

        public WinGetProxyHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
        }

        public override HashSet<string> HandledNamespaces => [NAMESPACE_WINGET];

        public override string DisplayName => "WinGet";

        public bool IsInitialized { get; private set; }

        internal IWinGetImplementation Implementation { get; private set; }

        public override async IAsyncEnumerable<PackageBase> GetFeaturedPackagesAsync()
        {
            yield break;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            string ns = packageUrn.NamespaceIdentifier;
            string id = packageUrn.GetContent();

            return await GetPackage(ns, id, status);
        }

        public async Task<PackageBase> GetPackage(string ns, string id, PackageStatus status = PackageStatus.Details)
        {
            if (ns == NAMESPACE_WINGET)
            {
                return await Implementation.GetPackage(id, this, status);
            }

            return null;
        }

        public override IAsyncEnumerable<PackageBase> GetSearchSuggestionsAsync(string query) => SearchAsync(query);

        public override IAsyncEnumerable<PackageBase> SearchAsync(string query) => Implementation.SearchAsync(query, this);

        public override async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            yield break;
        }

        public override ImageBase GetImage()
        {
            return new FileImage
            {
                Url = "https://github.com/microsoft/winget-cli/blob/master/.github/images/WindowsPackageManager_Assets/ICO/PNG/_64.png?raw=true"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            string ns = NAMESPACE_WINGET;
            string id = null;

            Regex rx = WinGetManifestRepoRx();
            Match m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["packageId"].Value;
                goto success;
            }

            rx = WinstallRx();
            m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["id"].Value;
                goto success;
            }

            rx = WinGetRunRx();
            m = rx.Match(url);
            if (m.Success)
            {
                id = $"{m.Groups["pub"].Value}.{m.Groups["pack"].Value}";
                goto success;
            }

            if (id == null) return null;

            success:
            return await GetPackage(new Urn(ns, new RawNamespaceSpecificString(id)));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            string path = package.Urn.NamespaceIdentifier switch
            {
                NAMESPACE_WINGET => "apps",
                _ => throw new ArgumentException()
            };

            Url url = "https://winstall.app".AppendPathSegments(path, package.Urn.GetContent());
            return url;
        }

        [GeneratedRegex("^https:\\/\\/((www\\.)?github|raw\\.githubusercontent)\\.com\\/microsoft\\/winget-pkgs(\\/(blob|tree))?\\/master\\/manifests\\/[0-9a-z]\\/(?<packageId>[^\\/\\s]+)()", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex WinGetManifestRepoRx();

        [GeneratedRegex(@"^https?://(www\\.)?winget\.run/pkg/(?<pub>[\w\d]+)/(?<pack>.+)", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex WinGetRunRx();

        [GeneratedRegex("^https?://(www\\.)?winstall\\.app/apps/(?<id>[^/\\s]+)\\??", RegexOptions.IgnoreCase, "en-US")]
        private static partial Regex WinstallRx();

        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
                return;

            Implementation = await Com.WinGetComHandler.TryCreateAsync();
            Implementation ??= new Cli.WinGetCliHandler();

            IsInitialized = true;
        }
    }
}

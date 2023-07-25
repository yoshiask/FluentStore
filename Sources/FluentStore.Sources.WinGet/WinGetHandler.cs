using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.Services;
using Winstall;
using Winstall.Models;
using FluentStore.SDK.Helpers;
using System.Linq;

namespace FluentStore.Sources.WinGet
{
    public class WinGetHandler : PackageHandlerBase
    {
        private readonly WinstallApi _api = new();

        public const string NAMESPACE_WINGET = "winget";
        public const string NAMESPACE_WINSTALL_PACK = "winstall-pack";

        public WinGetHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {

        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_WINGET,
            NAMESPACE_WINSTALL_PACK,
        };

        public override string DisplayName => "Winstall";

        public override async IAsyncEnumerable<PackageBase> GetFeaturedPackagesAsync()
        {
            var index = await _api.GetIndexAsync();

            foreach (PopularApp wgApp in index.Popular)
                yield return new WinGetPackage(this, popApp: wgApp)
                {
                    Status = PackageStatus.BasicDetails
                };

            foreach (Pack wgPack in index.Recommended)
                yield return new WinstallPack(this, wgPack)
                {
                    Status = PackageStatus.Details
                };
        }

        public override Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            string ns = packageUrn.NamespaceIdentifier;
            string id = packageUrn.GetContent();

            return GetPackage(ns, id, status);
        }

        public async Task<PackageBase> GetPackage(string ns, string id, PackageStatus status = PackageStatus.Details)
        {
            if (ns == NAMESPACE_WINGET)
            {
                // WinGet package in the WPM Community Repo
                var result = await _api.GetAppAsync(id);
                WinGetPackage package = new(this, result.App)
                {
                    Status = PackageStatus.Details
                };

                if (status.IsAtLeast(PackageStatus.Details))
                {
                    var locale = await CommunityRepo.GetDefaultLocaleAsync(id, result.App.LatestVersion);
                    package.Update(locale);
                    package.Status = PackageStatus.Details;
                }

                return package;
            }
            else if (ns == NAMESPACE_WINSTALL_PACK)
            {
                // Winstall Pack: https://winstall.app/packs
                var result = await _api.GetPackAsync(id);
                WinstallPack collection = new(this, result.Pack, result.Creator)
                {
                    Status = PackageStatus.Details
                };

                return collection;
            }

            return null;
        }

        public override IAsyncEnumerable<PackageBase> GetSearchSuggestionsAsync(string query) => SearchAsync(query);

        public override async IAsyncEnumerable<PackageBase> SearchAsync(string query)
        {
            var results = await _api.SearchAppsAsync(query);
            foreach (var wgApp in results)
                yield return new WinGetPackage(this, wgApp);
        }

        public override async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            var result = await _api.GetPacksAsync();
            foreach (var p in result.Packs.Take(10))
                yield return new WinstallPack(this, p)
                {
                    Status = PackageStatus.Details
                };
        }

        public override ImageBase GetImage()
        {
            return new FileImage
            {
                Url = Constants.WINSTALL_HOST.AppendPathSegments("favicon.ico")
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            string ns = NAMESPACE_WINGET;
            string id = null;

            Regex rx = new(@"^https:\/\/((www\.)?github|raw\.githubusercontent)\.com\/microsoft\/winget-pkgs(\/(blob|tree))?\/master\/manifests\/[0-9a-z]\/(?<packageId>[^\/\s]+)()",
                RegexOptions.IgnoreCase);
            Match m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["packageId"].Value;
                goto success;
            }

            rx = new(@"^https?://(www\.)?winstall\.app/(?<type>apps|packs)/(?<id>[^/\s]+)\??", RegexOptions.IgnoreCase);
            m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["id"].Value;

                if (m.Groups["type"].Value.ToUpperInvariant() == "PACKS")
                    ns = NAMESPACE_WINSTALL_PACK;

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
                NAMESPACE_WINSTALL_PACK => "packs",
                _ => throw new System.ArgumentException()
            };

            Url url = Constants.WINSTALL_HOST.AppendPathSegments(path, package.Urn.GetContent());
            return url;
        }
    }
}

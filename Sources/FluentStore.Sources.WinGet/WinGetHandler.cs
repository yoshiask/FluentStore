using FluentStore.SDK.Images;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.Services;
using Winstall;
using Winstall.Models;
using FluentStore.SDK.Helpers;

namespace FluentStore.Sources.WinGet
{
    public class WinGetHandler : PackageHandlerBase
    {
        private readonly WinstallApi _api = new();

        public const string NAMESPACE_WINGET = "winget";

        public WinGetHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {

        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_WINGET
        };

        public override string DisplayName => "WinGet";

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            var packages = new List<PackageBase>();
            var index = await _api.GetIndexAsync();
            foreach (PopularApp wgApp in index.Popular)
                packages.Add(new WinGetPackage(this, popApp: wgApp)
                {
                    Status = PackageStatus.BasicDetails
                });

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_WINGET, nameof(packageUrn));

            string packageId = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            var result = await _api.GetAppAsync(packageId);
            WinGetPackage package = new(this, result.App)
            {
                Status = PackageStatus.Details
            };

            if (status.IsAtLeast(PackageStatus.Details))
            {
                var locale = await CommunityRepo.GetDefaultLocaleAsync(packageId, result.App.LatestVersion);
                package.Update(locale);
                package.Status = PackageStatus.Details;
            }

            return package;
        }

        public override Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            return SearchAsync(query);
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();

            var results = await _api.SearchAppsAsync(query);
            foreach (App wgApp in results)
                packages.Add(new WinGetPackage(this, wgApp));

            return packages;
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
            string id = null;
            Regex rx = new(@"^https:\/\/((www\.)?github|raw\.githubusercontent)\.com\/microsoft\/winget-pkgs(\/(blob|tree))?\/master\/manifests\/[0-9a-z]\/(?<packageId>[^\/\s]+)()",
                RegexOptions.IgnoreCase);
            Match m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["packageId"].Value;
                goto success;
            }

            rx = new(@"^https?://(www\.)?winstall\.app/apps/(?<id>[^/\s]+)\??", RegexOptions.IgnoreCase);
            m = rx.Match(url);
            if (m.Success)
            {
                id = m.Groups["id"].Value;
                goto success;
            }

            if (id == null) return null;

        success:
            return await GetPackage(new Urn(NAMESPACE_WINGET, new RawNamespaceSpecificString(id)));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is not WinGetPackage wgPackage)
                throw new System.ArgumentException();

            Url url = Constants.WINSTALL_HOST.AppendPathSegments("apps", wgPackage.WinGetId);

            return url;
        }

        internal WinstallApi GetApiClient() => _api;
    }
}

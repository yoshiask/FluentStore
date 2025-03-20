using Chocolatey;
using Chocolatey.Cli;
using CommunityToolkit.Diagnostics;
using FluentStore.SDK;
using FluentStore.SDK.Images;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Sources.Chocolatey
{
    public partial class ChocolateyHandler : PackageHandlerBase
    {
        public const string NAMESPACE_CHOCO = "choco";

        private readonly IChocoSearchService _search;
        private readonly IChocoPackageService _pkgMan;

        public ChocolateyHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            _search = new ChocoCommunityWebClient();
            _pkgMan = new ChocoAdminCliClient();
        }

        public override HashSet<string> HandledNamespaces => [NAMESPACE_CHOCO];

        public override string DisplayName => "Chocolatey";

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_CHOCO, nameof(packageUrn));

            var urnStr = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;

            string id;
            NuGetVersion version = null;

            int versionIdx = urnStr.LastIndexOf(':');
            if (versionIdx > 0)
            {
                _ = NuGetVersion.TryParse(urnStr[(versionIdx + 1)..], out version);
                id = urnStr[..versionIdx];
            }
            else
            {
                id = urnStr;
            }

            var package = await _search.GetPackageAsync(id, version);

            return new ChocolateyPackage(this, _pkgMan, package);
        }

        public override async IAsyncEnumerable<PackageBase> SearchAsync(string query)
        {
            var results = await _search.SearchAsync(query);

            foreach (var chocoPackage in results)
                yield return new ChocolateyPackage(this, _pkgMan, chocoPackage);
        }

        public override ImageBase GetImage() => new FileImage("https://github.com/chocolatey/choco-theme/blob/main/images/global-shared/logo.png?raw=true");

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            if (!url.Authority.Equals("community.chocolatey.org", StringComparison.InvariantCultureIgnoreCase)
                || url.PathSegments.Count < 2
                || !url.PathSegments[0].Equals("packages", StringComparison.InvariantCultureIgnoreCase))
                return null;

            string id = url.PathSegments[1];
            string urn = $"urn:{NAMESPACE_CHOCO}:{id}";

            if (url.PathSegments.Count >= 3)
                urn += $":{url.PathSegments[2]}";

            return await GetPackage(Urn.Parse(urn));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is not ChocolateyPackage chocoPackage)
                throw new ArgumentException($"Package must be of type {nameof(ChocolateyPackage)}", nameof(package));

            string url = $"https://community.chocolatey.org/packages/{chocoPackage.PackageId}";

            if (chocoPackage.Version != null)
                url += $"/{chocoPackage.Version}";

            return url;
        }
    }
}

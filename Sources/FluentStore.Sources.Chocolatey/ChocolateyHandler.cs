using Chocolatey;
using Chocolatey.Models;
using CommunityToolkit.Diagnostics;
using FluentStore.SDK;
using FluentStore.SDK.Images;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluentStore.Sources.Chocolatey
{
    public class ChocolateyHandler : PackageHandlerBase
    {
        public const string NAMESPACE_CHOCO = "choco";

        public ChocolateyHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_CHOCO
        };

        public override string DisplayName => "Chocolatey";

        public override Task<List<PackageBase>> GetFeaturedPackagesAsync() => Task.FromResult(_emptyPackageList);

        public override async Task<PackageBase> GetPackage(Urn packageUrn, SDK.PackageStatus status = SDK.PackageStatus.Details)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_CHOCO, nameof(packageUrn));

            // TODO: Support getting packages without specifying a version
            var urnStr = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            int versionIdx = urnStr.LastIndexOf(':');
            if (versionIdx <= 0)
                throw new NotSupportedException("The choco client library does not currently support fetching package info without specifying a version.");
            var package = await Choco.GetPackageAsync(urnStr[..versionIdx], Version.Parse(urnStr[(versionIdx + 1)..]));

            return new ChocolateyPackage(this, package);
        }

        public override Task<List<PackageBase>> GetSearchSuggestionsAsync(string query) => Task.FromResult(_emptyPackageList);

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            List<PackageBase> packages = new();
            var results = await Choco.SearchAsync(query);
            foreach (Package chocoPackage in results)
                packages.Add(new ChocolateyPackage(this, chocoPackage));

            return packages;
        }

        public override ImageBase GetImage()
        {
            return new TextImage
            {
                Text = "Ch",
                FontFamily = "Segoe UI Variable Display"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            Regex rx = new(@"^https?:\/\/community\.chocolatey\.org\/packages\/(?<id>[^\/\s]+)(?:\/(?<version>[\d.]+))?",
                RegexOptions.IgnoreCase);
            Match m = rx.Match(url.ToString());
            if (!m.Success)
                return null;

            string urn = $"urn:{NAMESPACE_CHOCO}:{m.Groups["id"]}";
            var versionGroup = m.Groups["version"];
            if (versionGroup.Success)
                urn += "." + versionGroup.Value;

            return await GetPackage(Urn.Parse(urn));
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is not ChocolateyPackage chocoPackage)
                throw new ArgumentException($"Package must be of type {nameof(ChocolateyPackage)}", nameof(package));

            string url = $"https://community.chocolatey.org/packages/{chocoPackage.PackageId}";

            if (chocoPackage.Version != null)
                url += "/" + chocoPackage.Version;

            return url;
        }
    }
}

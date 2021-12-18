using CommunityToolkit.Diagnostics;
using FluentStore.SDK.Images;
using FluentStore.SDK.PackageTypes;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK.Handlers
{
    public class GitHubHandler : PackageHandlerBase
    {
        // TODO: Users will need to sign in to avoid the rate limit issues

        private static readonly GitHubClient client = new(new ProductHeaderValue("fluent-store"));

        public const string NAMESPACE_REPO = "gh-repo";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_REPO
        };

        public override string DisplayName => "GitHub";

        public override Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            return Task.FromResult(new List<PackageBase>());
        }

        public override ImageBase GetImage()
        {
            return new FileImage("ms-appx:///Assets/PackageHandlerIcons/GitHubHandler/GitHub-Mark.png");
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_REPO, nameof(packageUrn));

            var parts = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue.Split(new[] { '/', '\\', ':'}, 2);
            string owner = parts[0];
            string name = parts[1];
            var repo = await client.Repository.Get(owner, name);

            return new GitHubPackage(repo) { Status = PackageStatus.Details };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            if (url.Host == "github.com" && url.PathSegments.Count > 2)
            {
                string owner = url.PathSegments[^2];
                string name = url.PathSegments[^1];
                return await GetPackage(Urn.Parse($"urn:{NAMESPACE_REPO}:{owner}:{name}"));
            }

            return null;
        }

        public override Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            throw new NotImplementedException();
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            return $"https://github.com/{package.PublisherId}/{package.Title}";
        }

        public override Task<List<PackageBase>> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }

        public static Task<IReadOnlyList<Release>> GetReleases(Repository repo)
        {
            return client.Repository.Release.GetAll(repo.Id);
        }
    }
}

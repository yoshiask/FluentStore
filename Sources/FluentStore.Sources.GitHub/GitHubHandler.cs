using CommunityToolkit.Diagnostics;
using FluentStore.SDK;
using FluentStore.SDK.Images;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Sources.GitHub
{
    public class GitHubHandler : PackageHandlerBase
    {
        public const string NAMESPACE_REPO = "gh-repo";

        private static readonly Users.CredentialStore _credentialStore = new();
        private static readonly GitHubClient _client = new(new ProductHeaderValue("fluent-store"), _credentialStore);

        public GitHubHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            AccountHandler = new Users.GitHubAccountHandler(_client, _credentialStore, passwordVaultService);
        }

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

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus status = PackageStatus.Details)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_REPO, nameof(packageUrn));

            var parts = packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue.Split(new[] { '/', '\\', ':'}, 2);
            string owner = parts[0];
            string name = parts[1];
            var repo = await _client.Repository.Get(owner, name);

            return new GitHubPackage(this, repo) { Status = PackageStatus.Details };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            if (url.Host == "github.com" && url.PathSegments.Count >= 2)
            {
                string owner = url.PathSegments[0];
                string name = url.PathSegments[1];
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
            return _client.Repository.Release.GetAll(repo.Id);
        }
    }
}

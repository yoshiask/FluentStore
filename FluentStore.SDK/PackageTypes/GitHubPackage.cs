using CommunityToolkit.Diagnostics;
using FluentStore.SDK.Attributes;
using FluentStore.SDK.Images;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentStore.SDK.PackageTypes
{
    internal class GitHubPackage : PackageBase<Repository>
    {
        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse($"urn:{Handlers.GitHubHandler.NAMESPACE_REPO}:{Model.Owner.Login}:{Model.Name}");
                return _Urn;
            }
            set => _Urn = value;
        }

        public GitHubPackage(Repository repo = null)
        {
            if (repo != null)
                Update(repo);
        }

        public void Update(Repository repo)
        {
            Guard.IsNotNull(repo, nameof(repo));
            Model = repo;

            // Set base properties
            Model = repo;
            Title = repo.Name;
            DeveloperName = repo.Owner.Name ?? repo.Owner.Login;
            PublisherId = repo.Owner.Login;
            Description = repo.Description;
            DisplayPrice = "Free";
            ReleaseDate = repo.CreatedAt.ToLocalTime();
            if (!string.IsNullOrEmpty(repo.Homepage))
                Website = new(repo.Homepage, repo.Name + " website");
        }

        public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(TextImage.CreateFromName(Title, ImageType.Logo));

        public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

        public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult(new List<ImageBase>());

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> InstallAsync()
        {
            throw new NotImplementedException();
        }

        public override Task LaunchAsync()
        {
            throw new NotImplementedException();
        }

        [DisplayAdditionalInformation("Open issues", "\u2609", FontFamily = "Segoe UI Symbol")]
        public string OpenIssuesCount => Model.OpenIssuesCount.ToString("N0");

        [DisplayAdditionalInformation("Stargazers", "\uE734")]
        public string StarsCount => Model.StargazersCount.ToString("N0");

        [DisplayAdditionalInformation("Watching", "\uE7B3")]
        public string WatchersCount => Model.WatchersCount.ToString("N0");

        [DisplayAdditionalInformation("Forks", "\u2ADD", FontFamily = "Segoe UI Symbol")]
        public string ForksCount => Model.ForksCount.ToString("N0");

        private List<Link> _Links;
        [DisplayAdditionalInformation(Icon = "\uE71B")]
        public List<Link> Links
        {
            get
            {
                if (_Links == null)
                {
                    _Links = new();
                    if (!string.IsNullOrEmpty(Model.CloneUrl))
                        _Links.Add(new(Model.CloneUrl, "Clone (HTTPS)"));
                    if (!string.IsNullOrEmpty(Model.GitUrl))
                        _Links.Add(new(Model.GitUrl, "Clone (Git)"));
                    if (!string.IsNullOrEmpty(Model.SvnUrl))
                        _Links.Add(new(Model.SvnUrl, "Clone (SVN)"));
                }
                return _Links;
            }
            set => SetProperty(ref _Links, value);
        }

    }
}

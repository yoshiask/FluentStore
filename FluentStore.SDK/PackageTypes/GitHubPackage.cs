using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Attributes;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public GitHubPackage(Repository repo = null, IReadOnlyList<Release> releases = null)
        {
            if (repo != null)
                Update(repo);
            if (releases != null)
                Update(releases);
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

        public void Update(IReadOnlyList<Release> releases)
        {
            Guard.IsNotNull(releases, nameof(releases));
            Releases = releases;
        }

        public override Task<ImageBase> CacheAppIcon() => Task.FromResult<ImageBase>(TextImage.CreateFromName(Title, ImageType.Logo));

        public override Task<ImageBase> CacheHeroImage() => Task.FromResult<ImageBase>(null);

        public override Task<List<ImageBase>> CacheScreenshots() => Task.FromResult(new List<ImageBase>());

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            ReleaseAsset asset = null;
            Release release = null;
            try
            {
                // Fetch assets
                if (Releases == null)
                    Releases = await Handlers.GitHubHandler.GetReleases(Model);

                // Find suitable release asset
                string[] architecture_strings = { "x64", "amd64", "x86", "arm", "arm64" };
                foreach (Release rel in Releases)
                {
                    if (rel.Assets.Count == 0) continue;

                    // Pick proper asset for architecture
                    IEnumerable<ReleaseAsset> assets = rel.Assets;
                    if (assets.Any(a => a.Name.Contains(StringComparison.InvariantCultureIgnoreCase, architecture_strings)))
                    {
                        // Some assets are architecture-specific
                        var arch = Win32Helper.GetSystemArchitecture();

                        // Exclude mismatched architectures, but keep assets that don't specify an architecture
                        assets = assets.Where(a =>
                            a.Name.Contains(arch.ToString(), StringComparison.InvariantCultureIgnoreCase)
                            || !a.Name.Contains(StringComparison.InvariantCultureIgnoreCase, architecture_strings));
                    }
                    if (rel.Assets.Count == 0) continue;
                    else if (assets.Count() == 1)
                    {
                        release = rel;
                        asset = assets.First();
                        break;
                    }

                    // Rank assets by file type
                    int RankAsset(ReleaseAsset a) => this.RankAsset(a.Name);
                    assets = assets.OrderBy(RankAsset);
                    asset = assets.First();
                    release = rel;
                    break;
                }

                if (asset == null)
                    throw WebException.Create(404, "No packages are available for " + ShortTitle);

                PackageUri = new(asset.BrowserDownloadUrl);
                Version = release.TagName;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageFetchFailed));
                return null;
            }

            try
            {
                // Download chosen asset
                await StorageHelper.BackgroundDownloadPackage(this, PackageUri, folder);

                // Check for success
                if (Status.IsLessThan(PackageStatus.Downloaded))
                    return null;

                if (PackageUri != null && DownloadItem is FileInfo file)
                    DownloadItem = file.CopyRename(Path.GetFileName(PackageUri.AbsolutePath));
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageDownloadFailed));
                return null;
            }

            Type = InstallerTypes.FromExtension(DownloadItem.Extension[1..]);
            Status = PackageStatus.Downloaded;
            return DownloadItem;
        }

        public override async Task<bool> InstallAsync()
        {
            // Make sure installer is downloaded
            Guard.IsTrue(Status.IsAtLeast(PackageStatus.Downloaded), nameof(Status));
            bool success = false;

            try
            {
                if (Type == InstallerType.Zip && DownloadItem is FileInfo archiveFile)
                {
                    // ZIP is a special type, because it itself is not an installer.
                    using FileStream archiveStream = archiveFile.OpenRead();
                    using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read);

                    // Pick file that is most likely an installer
                    // Only look at top-level files
                    int RankAsset(ZipArchiveEntry e) => this.RankAsset(e.Name);
                    var assets = archive.Entries.Where(e => !e.FullName.Contains('/')).OrderBy(RankAsset);
                    if (!assets.Any())
                        throw new Exception("Failed to find a good installer candidate for " + Title);

                    // Extract everything, but keep track of installer
                    string installerFilename = assets.First().Name;
                    var dir = new DirectoryInfo(archiveFile.FullName[..^archiveFile.Extension.Length]);
                    archive.ExtractToDirectory(dir.FullName, true);
                    DownloadItem = new FileInfo(Path.Combine(dir.FullName, installerFilename));
                    Type = InstallerTypes.FromExtension(Path.GetExtension(installerFilename)[1..]);
                }

                InstallerType typeReduced = Type.Reduce();
                if (typeReduced == InstallerType.Win32)
                    success = await Win32Helper.Install(this);
                else if (typeReduced == InstallerType.Msix)
                    success = await PackagedInstallerHelper.Install(this);
                else
                    throw new Exception("Cannot install " + DownloadItem.Name);
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, this, ErrorType.PackageInstallFailed));
                return false;
            }

            if (success)
                Status = PackageStatus.Installed;
            return success;
        }

        private int RankAsset(string filename)
        {
            int extIdx = filename.LastIndexOf('.');
            if (extIdx < 0) return int.MaxValue;

            string ext = filename[(extIdx + 1)..];
            int score = ext.ToUpperInvariant() switch
            {
                "APPINSTALLER" => 0,
                "MSIXBUNDLE" => 1,
                "APPXBUNDLE" => 2,
                "MSIX" => 3,
                "APPX" => 4,
                "MSI" => 5,
                "EXE" => 6,
                "ZIP" => 7,

                // Default to MaxInt - 10, so if the asset contains the name of the repo, we don't overflow
                _ => 0x7FFFFFF5
            };
            if (!filename.Contains(Title, StringComparison.InvariantCultureIgnoreCase))
                score += 10;
            return score;
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

        private IReadOnlyList<Release> _Releases;
        public IReadOnlyList<Release> Releases
        {
            get => _Releases;
            set => SetProperty(ref _Releases, value);
        }
    }
}

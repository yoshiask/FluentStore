using FluentStore.SDK.Images;
using FluentStore.Services;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK.Messages;
using FluentStore.SDK.Models;
using FluentStore.SDK.Attributes;
using FluentStore.SDK;
using Flurl.Util;
using FluentStore.Sources.UwpCommunity.Models;

namespace FluentStore.Sources.UwpCommunity
{
    public class UwpCommunityPackage : PackageBase<Project>
    {
        readonly NavigationServiceBase NavigationService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
        readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public UwpCommunityPackage(PackageHandlerBase packageHandler, Project project = null, IEnumerable<string> images = null,
            IEnumerable<Collaborator> collaborators = null, IEnumerable<string> features = null) : base(packageHandler)
        {
            if (project != null)
                UpdateWithProject(project);
            if (images != null)
                UpdateWithImages(images);
            if (collaborators != null)
                UpdateWithCollaborators(collaborators);
            if (features != null)
                UpdateWithFeatures(features);
        }

        public void UpdateWithProject(Project project)
        {
            Guard.IsNotNull(project, nameof(project));
            Model = project;

            // Set base properties
            Title = project.AppName;
            Description = project.Description;
            ReleaseDate = project.CreatedAt;
            Price = 0.0;
            DisplayPrice = "View";

            if (project.ExternalLink is string externalLink)
                Website = Link.Create(externalLink, ShortTitle + " website");

            if (project.HeroImage is string heroImage)
                Images.Add(new FileImage
                {
                    Url = heroImage,
                    ImageType = ImageType.Hero
                });

            if (project.AppIcon is string appIcon)
                Images.Add(new FileImage
                {
                    Url = appIcon,
                    ImageType = ImageType.Logo,
                    BackgroundColor = project.AccentColor,
                });

            // Set UWPC properties
            ProjectId = project.Id;

            if (project.DownloadLink is string downloadLink)
                PackageUri = new(downloadLink);

            if (project.GithubLink is string githubLink)
                GithubLink = Link.Create(githubLink, ShortTitle + " on GitHub");

            if (project.Tags != null)
                foreach (var tag in project.Tags)
                    Tags.Add(tag.Name);

            Urn = new(UwpCommunityHandler.NAMESPACE_PROJECT, new RawNamespaceSpecificString(ProjectId.ToInvariantString()));
        }

        public void UpdateWithImages(IEnumerable<string> images)
        {
            Guard.IsNotNull(images, nameof(images));

            foreach (string img in images)
            {
                Images.Add(new FileImage
                {
                    Url = img,
                    ImageType = ImageType.Screenshot
                });
            }
        }

        public void UpdateWithCollaborators(IEnumerable<Collaborator> collaborators)
        {
            Guard.IsNotNull(collaborators, nameof(collaborators));

            var owner = collaborators.FirstOrDefault(c => c.IsOwner);
            if (owner != null)
                DeveloperName = owner.Name;
        }

        public void UpdateWithFeatures(IEnumerable<string> features)
        {
            Guard.IsNotNull(features, nameof(features));

            Features.AddRange(features);
        }

        public override async Task<bool> CanLaunchAsync()
        {
            if (LinkedPackage != null)
                return await LinkedPackage.CanLaunchAsync();
            else
                return false;
        }

        public override async Task<bool> CanDownloadAsync()
        {
            if (PackageUri is null)
                return false;

            LinkedPackage = await PackageService.GetPackageFromUrlAsync(PackageUri);
            if (LinkedPackage is null)
                return false;

            return await LinkedPackage.CanDownloadAsync();
        }

        public override async Task<FileSystemInfo> DownloadAsync(DirectoryInfo folder = null)
        {
            if (PackageUri == null)
            {
                // No downlod link is available
                WeakReferenceMessenger.Default.Send(new ErrorMessage(
                    new Exception($"There are no download links available for {Title}."), this, ErrorType.PackageFetchFailed));
                return null;
            }

            LinkedPackage ??= await PackageService.GetPackageFromUrlAsync(PackageUri);
            if (LinkedPackage != null)
            {
                DownloadItem = await LinkedPackage.DownloadAsync(folder);
                Status = LinkedPackage.Status;
                return DownloadItem;
            }
            else
            {
                if (await NavigationService.OpenInBrowser(PackageUri))
                    IsDownloaded = true;
                return null;
            }
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            var icon = Images.FirstOrDefault(i => i.ImageType == ImageType.Logo)
                ?? TextImage.CreateFromName(Title);

            if (LinkedPackage != null)
                LinkedPackage.AppIconCache = icon;
            return icon;
        }

        public override async Task<ImageBase> CacheHeroImage()
        {
            var img = Images.FirstOrDefault(i => i.ImageType == ImageType.Hero);

            if (LinkedPackage != null)
                LinkedPackage.HeroImageCache = img;
            return img;
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            var screenhots = Images.Where(i => i.ImageType == ImageType.Screenshot).ToList();

            if (LinkedPackage != null)
                LinkedPackage.ScreenshotsCache = screenhots;
            return screenhots;
        }

        public override async Task<bool> InstallAsync()
        {
            if (LinkedPackage != null)
                return await LinkedPackage.InstallAsync();
            else
                return true;
        }

        public override async Task LaunchAsync()
        {
            if (LinkedPackage != null)
                await LinkedPackage.LaunchAsync();
        }

        private int _ProjectId;
        public int ProjectId
        {
            get => _ProjectId;
            set => SetProperty(ref _ProjectId, value);
        }

        private PackageBase _LinkedPackage;
        public PackageBase LinkedPackage
        {
            get => _LinkedPackage;
            set => SetProperty(ref _LinkedPackage, value);
        }

        private Link _GithubLink;
        [DisplayAdditionalInformation("Source code", "\uE943")]
        public Link GithubLink
        {
            get => _GithubLink;
            set => SetProperty(ref _GithubLink, value);
        }

        private List<string> _Tags = new();
        [DisplayAdditionalInformation(Icon = "\uE1CB")]
        public List<string> Tags
        {
            get => _Tags;
            set => SetProperty(ref _Tags, value);
        }

        private List<string> _Features = new();
        [Display(Rank = 2)]
        public List<string> Features
        {
            get => _Features;
            set => SetProperty(ref _Features, value);
        }
    }
}

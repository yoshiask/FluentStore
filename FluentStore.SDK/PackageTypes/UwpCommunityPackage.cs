using FluentStore.SDK.Images;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace FluentStore.SDK.Packages
{
    public class UwpCommunityPackage : PackageBase<dynamic>
    {
        INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
        PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public UwpCommunityPackage(dynamic project = null, IEnumerable<string> images = null, IEnumerable<dynamic> collaborators = null)
        {
            if (project != null)
                UpdateWithProject(project);
            if (images != null)
                UpdateWithImages(images);
            if (collaborators != null)
                UpdateWithCollaborators(collaborators);
        }

        public void UpdateWithProject(dynamic project)
        {
            Guard.IsNotNull(project, nameof(project));
            Model = project;

            // Set base properties
            Title = project.appName;
            Description = project.description;
            ReleaseDate = project.createdAt;

            if (project.heroImage != null)
                Images.Add(new FileImage
                {
                    Url = project.heroImage,
                    ImageType = ImageType.Hero
                });

            if (project.appIcon != null)
                Images.Add(new FileImage
                {
                    Url = project.appIcon,
                    ImageType = ImageType.Logo,
                    BackgroundColor = project.accentColor,
                });
            else
                Images.Add(TextImage.CreateFromName(Title));

            // Set UWPC properties
            ProjectId = (int)project.id;
            DownloadLink = project.downloadLink;
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

        public void UpdateWithCollaborators(IEnumerable<dynamic> collaborators)
        {
            Guard.IsNotNull(collaborators, nameof(collaborators));

            dynamic owner = collaborators.FirstOrDefault(c => c.isOwner == true);
            if (owner != null)
                DeveloperName = owner.name;
        }

        private Urn _Urn;
        public override Urn Urn
        {
            get
            {
                if (_Urn == null)
                    _Urn = Urn.Parse("urn:" + Handlers.UwpCommunityHandler.NAMESPACE_PROJECT + ":" + ProjectId);
                return _Urn;
            }
            set => _Urn = value;
        }

        public override async Task<bool> CanLaunchAsync()
        {
            if (LinkedPackage != null)
                return await LinkedPackage.CanLaunchAsync();
            else
                return false;
        }

        public override async Task<FileSystemInfo> DownloadPackageAsync(DirectoryInfo folder = null)
        {
            LinkedPackage = await PackageService.GetPackageFromUrlAsync(DownloadLink);
            if (LinkedPackage != null)
            {
                DownloadItem = await LinkedPackage.DownloadPackageAsync(folder);
                Status = LinkedPackage.Status;
                return DownloadItem;
            }
            else
            {
                Status = await NavigationService.OpenInBrowser(DownloadLink)
                    ? PackageStatus.Downloaded : PackageStatus.DownloadReady;
                return null;
            }
        }

        public override async Task<ImageBase> CacheAppIcon()
        {
            return Images.FirstOrDefault(i => i.ImageType == ImageType.Logo)
                ?? TextImage.CreateFromName(Title);
        }

        public override async Task<ImageBase> CacheHeroImage()
        {
            return Images.FirstOrDefault(i => i.ImageType == ImageType.Hero);
        }

        public override async Task<List<ImageBase>> CacheScreenshots()
        {
            return Images.Where(i => i.ImageType == ImageType.Screenshot).ToList();
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

        private Url _DownloadLink;
        public Url DownloadLink
        {
            get => _DownloadLink;
            set => SetProperty(ref _DownloadLink, value);
        }

        private PackageBase _LinkedPackage;
        public PackageBase LinkedPackage
        {
            get => _LinkedPackage;
            set => SetProperty(ref _LinkedPackage, value);
        }
    }
}

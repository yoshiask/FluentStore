using FluentStore.SDK.Images;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

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

        public override Task<bool> CanLaunchAsync() => Task.FromResult(false);

        public override async Task<IStorageItem> DownloadPackageAsync(StorageFolder folder = null)
        {
            LinkedPackage = await PackageService.GetPackageFromUrlAsync(DownloadLink);
            if (LinkedPackage != null)
            {
                return await LinkedPackage.DownloadPackageAsync(folder);
            }
            else
            {
                Status = await NavigationService.OpenInBrowser(DownloadLink)
                    ? PackageStatus.Downloaded : PackageStatus.DownloadReady;
                return null;
            }
        }

        public override async Task<ImageBase> GetAppIcon()
        {
            return Images.FirstOrDefault(i => i.ImageType == ImageType.Logo)
                ?? TextImage.CreateFromName(Title);
        }

        public override async Task<ImageBase> GetHeroImage()
        {
            return Images.FirstOrDefault(i => i.ImageType == ImageType.Hero);
        }

        public override async Task<List<ImageBase>> GetScreenshots()
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

        public override Task LaunchAsync() => throw new NotImplementedException();

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

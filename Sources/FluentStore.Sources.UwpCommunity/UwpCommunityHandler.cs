using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Flurl;
using Flurl.Http;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using FluentStore.Sources.UwpCommunity.Models;

namespace FluentStore.Sources.UwpCommunity
{
    public class UwpCommunityHandler : PackageHandlerBase
    {
        // TODO: Switch web requests to official UWPC library when available

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private const string BASE_URL = "https://uwpcommunity-site-backend.herokuapp.com";

        public const string NAMESPACE_PROJECT = "uwpc-projects";
        public const string NAMESPACE_LAUNCH = "uwpc-launch";

        public UwpCommunityHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            // TODO: Create UWPC account handler
            //AccountHandler = new Users.UwpCommunityAccountHandler(passwordVaultService);
        }

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_PROJECT,
            NAMESPACE_LAUNCH
        };

        public override string DisplayName => "UWP Community";

        public override async IAsyncEnumerable<PackageBase> GetFeaturedPackagesAsync()
        {
            string launchYearStr = GetLastLaunchYear().ToString();
            var launchEvent = await BASE_URL.AppendPathSegments("projects", "launch", launchYearStr)
                .GetJsonAsync<LaunchEvent>();

            foreach (var project in launchEvent.Projects)
            {
                yield return new UwpCommunityPackage(this, project)
                {
                    Status = PackageStatus.BasicDetails
                };
            }
        }

        public override async Task<PackageBase> GetPackage(Urn urn, PackageStatus status)
        {
            string id = urn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            if (urn.NamespaceIdentifier == NAMESPACE_PROJECT)
                return await GetPackage(id, status);
            else if (urn.NamespaceIdentifier == NAMESPACE_LAUNCH)
                return await GetLaunchCollection(id, status);

            return null;
        }

        public async Task<PackageBase> GetPackage(string projectIdStr, PackageStatus status = PackageStatus.Details)
        {
            int projectId = int.Parse(projectIdStr);

            var request = BASE_URL.AppendPathSegments("projects");
            var projects = await request.GetJsonAsync<List<Project>>();
            var project = projects.FirstOrDefault(p => p.Id == projectId);
            if (project is null)
            {
                //var NavService = Ioc.Default.GetRequiredService<INavigationService>();
                throw SDK.Models.WebException.Create(404, $"No project with ID {projectId} is registered with the UWP Community.", request.ToString());
            }

            UwpCommunityPackage package = new(this, project);
            package.Status = PackageStatus.BasicDetails;

            if (status.IsAtLeast(PackageStatus.Details))
            {
                var images = await BASE_URL.AppendPathSegments("projects", "images")
                    .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<string>>();
                package.UpdateWithImages(images);

                var collaborators = await BASE_URL.AppendPathSegments("projects", "collaborators")
                    .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<Collaborator>>();
                package.UpdateWithCollaborators(collaborators);

                var features = await BASE_URL.AppendPathSegments("projects", "features")
                    .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<string>>();
                package.UpdateWithFeatures(features);

                package.Status = PackageStatus.DownloadReady;
            }

            return package;
        }

        public async Task<GenericPackageCollection<LaunchEvent>> GetLaunchCollection(string year, PackageStatus status)
        {
            var launchEvent = await BASE_URL.AppendPathSegments("projects", "launch", year).GetJsonAsync<LaunchEvent>();

            GenericPackageCollection<LaunchEvent> listPackage = new(this)
            {
                Urn = new(NAMESPACE_LAUNCH, new RawNamespaceSpecificString(year)),
                Title = "Launch " + year,
                Description = "An annual event hosted by the UWP Community, where developers, beta testers, translators, and users work together to Launch their new and refreshed apps.",
                DeveloperName = "UWP Community",
                Website = new("https://uwpcommunity.com/launch", "UWP Community Launch page"),
                Price = 0.0,
                DisplayPrice = "View",
                Images =
                {
                    new FileImage
                    {
                        Url = "https://github.com/UWPCommunity/UWPCommunityApp/blob/dev/UWPCommunity/Assets/StoreLogo.scale-400.png?raw=true",
                        ImageType = ImageType.Logo
                    }
                },
                Status = PackageStatus.BasicDetails,
                Model = launchEvent,
            };

            // Use showcase site when available
            if (year == "2021")
                listPackage.Website = new(listPackage.Website.Uri.AppendPathSegment("2021").ToUri(), "UWP Community Launch showcase");

            // Set hero image
            string heroImageUrl = $"https://uwpcommunity.com/launch/{year}/package/Assets/Banner.png";
            if (year == "2020")
                heroImageUrl = "https://uwpcommunity.com/assets/img/LaunchAppsHero.jpg";
            else if (year == "2019")
                heroImageUrl = "https://uwpcommunity.com/assets/img/LaunchHero.jpg";
            listPackage.Images.Add(new FileImage
            {
                Url = heroImageUrl,
                ImageType = ImageType.Hero
            });

            foreach (var project in launchEvent.Projects)
            {
                UwpCommunityPackage package = new(this, project);
                package.Status = PackageStatus.BasicDetails;
                listPackage.Items.Add(package);
            }

            listPackage.Status = PackageStatus.DownloadReady;
            return listPackage;
        }

        public override async IAsyncEnumerable<PackageBase> GetCollectionsAsync()
        {
            int lastLaunchYear = GetLastLaunchYear();
            for (int launchYear = 2019; launchYear <= lastLaunchYear; launchYear++)
            {
                var launchCollection = await GetLaunchCollection(launchYear.ToString(), PackageStatus.BasicDetails);
                if (launchCollection.Items.Count > 0)
                    yield return launchCollection;
            }
        }

        public override ImageBase GetImage()
        {
            return new FileImage
            {
                Url = "https://github.com/UWPCommunity/UWPCommunityApp/blob/dev/LaunchAssets/2020/AppIcon.png?raw=true"
            };
        }

        public override async Task<PackageBase> GetPackageFromUrl(Url url)
        {
            if (url.Host.EndsWith("uwpcommunity.com"))
            {
                if (url.QueryParams.TryGetFirst("project", out object projectId))
                {
                    return await GetPackage(projectId.ToString());
                }
            }

            return null;
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is UwpCommunityPackage uwpcPackage && uwpcPackage.HasWebsite)
                return uwpcPackage.Website;
            return "fluentstore://package/" + package.Urn.ToString();
        }

        private static int GetLastLaunchYear()
        {
            var now = DateTime.UtcNow;
            int launchYear = now.Year;
            if (now.Month < 9)
                launchYear--;
            return launchYear;
        }
    }
}

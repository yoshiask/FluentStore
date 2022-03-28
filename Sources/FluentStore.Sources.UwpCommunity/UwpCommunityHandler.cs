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

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            string launchYearStr = GetLastLaunchYear().ToString();
            var projects = (await BASE_URL.AppendPathSegments("projects", "launch", launchYearStr)
                .GetJsonAsync()).projects;

            List<PackageBase> packages = new(projects.Count);
            foreach (dynamic project in projects)
            {
                UwpCommunityPackage package = new(project)
                {
                    Status = PackageStatus.BasicDetails
                };
                packages.Add(package);
            }

            return packages;
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
            var projects = await request.GetJsonListAsync();
            dynamic project = projects.FirstOrDefault(p => p.id == projectId);
            if (project == null)
            {
                var NavService = Ioc.Default.GetRequiredService<Services.INavigationService>();
                throw SDK.Models.WebException.Create(404, $"No project with ID {projectId} is registered with the UWP Community.", request.ToString());
            }

            UwpCommunityPackage package = new(project);
            package.Status = PackageStatus.BasicDetails;

            if (status.IsAtLeast(PackageStatus.Details))
            {
                var images = await BASE_URL.AppendPathSegments("projects", "images")
                .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<string>>();
                package.UpdateWithImages(images);

                var collaborators = await BASE_URL.AppendPathSegments("projects", "collaborators")
                    .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<dynamic>>();
                package.UpdateWithCollaborators(collaborators);

                var features = await BASE_URL.AppendPathSegments("projects", "features")
                    .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<string>>();
                package.UpdateWithFeatures(features);

                package.Status = PackageStatus.DownloadReady;
            }
            
            return package;
        }

        public async Task<GenericPackageCollection<dynamic>> GetLaunchCollection(string year, PackageStatus status)
        {
            dynamic projects = (await BASE_URL.AppendPathSegments("projects", "launch", year).GetJsonAsync()).projects;

            GenericPackageCollection<dynamic> listPackage = new()
            {
                Urn = new(NAMESPACE_LAUNCH, new RawNamespaceSpecificString(year)),
                Title = "Launch " + year,
                Description = "An annual event hosted by the UWP Community, where developers, beta testers, translators, and users work together to Launch their new and refreshed apps.",
                DeveloperName = "UWP Community",
                Website = new("https://uwpcommunity.com/launch", "UWP Community Launch page"),
                Images =
                {
                    new FileImage
                    {
                        Url = "https://github.com/UWPCommunity/UWPCommunityApp/blob/dev/UWPCommunity/Assets/StoreLogo.scale-400.png?raw=true",
                        ImageType = ImageType.Logo
                    }
                },
                Status = PackageStatus.BasicDetails,
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

            foreach (dynamic project in projects)
            {
                UwpCommunityPackage package = new(project);
                package.Status = PackageStatus.BasicDetails;
                listPackage.Items.Add(package);
            }

            listPackage.Status = PackageStatus.DownloadReady;
            return listPackage;
        }

        public override Task<List<PackageBase>> GetSearchSuggestionsAsync(string query) => Task.FromResult(_emptyPackageList);

        public override Task<List<PackageBase>> SearchAsync(string query) => Task.FromResult(_emptyPackageList);

        public override async Task<List<PackageBase>> GetCollectionsAsync()
        {
            int lastLaunchYear = GetLastLaunchYear();
            List<PackageBase> collections = new(lastLaunchYear - 2019);

            for (int launchYear = 2019; launchYear <= lastLaunchYear; launchYear++)
            {
                var launchCollection = await GetLaunchCollection(launchYear.ToString(), PackageStatus.BasicDetails);
                collections.Add(launchCollection);
            }

            return collections;
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

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

namespace FluentStore.Sources.UwpCommunity
{
    public class UwpCommunityHandler : PackageHandlerBase
    {
        // TODO: Switch web requests to official UWPC library when available

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private const string BASE_URL = "https://uwpcommunity-site-backend.herokuapp.com";

        public const string NAMESPACE_PROJECT = "uwpc-projects";
        public const string NAMESPACE_LAUNCH = "uwpc-launch";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_PROJECT,
            NAMESPACE_LAUNCH
        };

        public override string DisplayName => "UWP Community";

        public override async Task<List<PackageBase>> GetFeaturedPackagesAsync()
        {
            var now = DateTime.UtcNow;
            int launchYear = now.Year;
            if (now.Month < 9)
                launchYear--;
            var projects = (await BASE_URL.AppendPathSegments("projects", "launch", launchYear.ToString())
                .GetJsonAsync()).projects;

            var packages = new List<PackageBase>(projects.Count);
            foreach (dynamic project in projects)
            {
                var package = new UwpCommunityPackage(project)
                {
                    Status = PackageStatus.BasicDetails
                };
                packages.Add(package);
            }

            return packages;
        }

        public override async Task<PackageBase> GetPackage(Urn urn)
        {
            string id = urn.GetContent<NamespaceSpecificString>().UnEscapedValue;
            if (urn.NamespaceIdentifier == NAMESPACE_PROJECT)
                return await GetPackage(id);
            else if (urn.NamespaceIdentifier == NAMESPACE_LAUNCH)
                return await GetLaunchCollection(id);

            return null;
        }

        public async Task<PackageBase> GetPackage(string projectIdStr)
        {
            int projectId = int.Parse(projectIdStr);

            var projects = await BASE_URL.AppendPathSegments("projects").GetJsonListAsync();
            dynamic project = projects.FirstOrDefault(p => p.id == projectId);
            if (project == null)
            {
                var NavService = Ioc.Default.GetRequiredService<Services.INavigationService>();
                NavService.ShowHttpErrorPage(404, "That project is not registered with the UWP Community.");
                return null;
            }

            UwpCommunityPackage package = new(project);
            package.Status = PackageStatus.BasicDetails;

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
            return package;
        }

        public async Task<GenericPackageCollection<dynamic>> GetLaunchCollection(string year)
        {
            dynamic projects = (await BASE_URL.AppendPathSegments("projects", "launch", year).GetJsonAsync()).projects;

            GenericPackageCollection<dynamic> listPackage = new()
            {
                Urn = new(NAMESPACE_LAUNCH, new RawNamespaceSpecificString(year)),
                Title = "Launch " + year,
                Description = "An annual event hosted by the UWP Community, where developers, beta testers, translators, and users work together to Launch their new and refreshed apps.",
                DeveloperName = "UWP Community",
                Website = new("https://uwpcommunity.com/launch", "UWP Community Launch page"),
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

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            // TODO
            return new List<PackageBase>(0);
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            // TODO
            return new List<PackageBase>(0);
        }

        public override ImageBase GetImage() => GetImageStatic();
        public static ImageBase GetImageStatic()
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

                // TODO: This code is here just in case the UWPC website gets more complicated.
                //Regex launchRx = new Regex(@"\/launch\/(?<launchYear>\d+)\/?\?project=(?<projId>\d+)");
                //Match launchMc = launchRx.Match(url);
                //if (launchMc.Success)
                //{
                //    return await GetPackage(launchMc.Groups["projId"].Value);
                //}

                //Regex projectsRx = new Regex(@"\/projects\/?(\?project=)?(?<projId>\d+)");
                //Match projectsMc = projectsRx.Match(url);
                //if (projectsMc.Success)
                //{
                //    return await GetPackage(launchMc.Groups["projId"].Value);
                //}
            }

            return null;
        }

        public override Url GetUrlFromPackage(PackageBase package)
        {
            if (package is UwpCommunityPackage uwpcPackage && uwpcPackage.HasWebsite)
                return uwpcPackage.Website;
            return "fluentstore://package/" + package.Urn.ToString();
        }
    }
}

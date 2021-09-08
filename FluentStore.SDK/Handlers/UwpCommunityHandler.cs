using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Flurl;
using Flurl.Http;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FSAPI = FluentStoreAPI.FluentStoreAPI;

namespace FluentStore.SDK.Handlers
{
    public class UwpCommunityHandler : PackageHandlerBase
    {
        // TODO: Switch web requests to official UWPC library when available

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private const string BASE_URL = "https://uwpcommunity-site-backend.herokuapp.com";

        public const string NAMESPACE_PROJECT = "uwpc-projects";
        public override HashSet<string> HandledNamespaces => new HashSet<string>
        {
            NAMESPACE_PROJECT,
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
            if (urn.NamespaceIdentifier == NAMESPACE_PROJECT)
                return await GetPackage(urn.GetContent<NamespaceSpecificString>().UnEscapedValue);

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

            var package = new UwpCommunityPackage(project);

            var images = await BASE_URL.AppendPathSegments("projects", "images")
                .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<string>>();
            package.UpdateWithImages(images);

            var collaborators = await BASE_URL.AppendPathSegments("projects", "collaborators")
                .SetQueryParam("projectId", projectIdStr).GetJsonAsync<List<dynamic>>();
            package.UpdateWithCollaborators(collaborators);

            package.Status = PackageStatus.DownloadReady;
            return package;
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
            return "fluentstore://package/" + package.Urn.ToString();
        }
    }
}

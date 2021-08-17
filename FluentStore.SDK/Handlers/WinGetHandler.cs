using FluentStore.SDK.Packages;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetRun;
using WinGetRun.Models;

namespace FluentStore.SDK.Handlers
{
    public class WinGetHandler : PackageHandlerBase
    {
        private readonly WinGetApi WinGetApi = Ioc.Default.GetRequiredService<WinGetApi>();

        public const string NAMESPACE_WINGET = "winget";
        public override HashSet<string> HandledNamespaces => new HashSet<string>
        {
            NAMESPACE_WINGET
        };

        public override async Task<PackageBase> GetPackage(Urn packageUrn)
        {
            Guard.IsEqualTo(packageUrn.NamespaceIdentifier, NAMESPACE_WINGET, nameof(packageUrn));

            var package = await WinGetApi.GetPackage(packageUrn.GetContent<NamespaceSpecificString>().UnEscapedValue);
            return new WinGetPackage(package);
        }

        public override async Task<List<PackageBase>> GetSearchSuggestionsAsync(string query)
        {
            var packages = new List<PackageBase>();
            var pageOptions = new PaginationOptions
            {
                Take = 3
            };
            var firstPage = await WinGetApi.SearchPackages(query: query, pageOptions: pageOptions);
            foreach (Package wgPackage in firstPage.Packages)
                packages.Add(new WinGetPackage(wgPackage));

            return packages;
        }

        public override async Task<List<PackageBase>> SearchAsync(string query)
        {
            var packages = new List<PackageBase>();
            var firstPage = await WinGetApi.SearchPackages(query: query);
            foreach (Package wgPackage in firstPage.Packages)
                packages.Add(new WinGetPackage(wgPackage));

            return packages;
        }
    }
}

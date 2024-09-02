using FluentStore.SDK.Helpers;
using FluentStore.SDK.Images;
using FluentStore.SDK.Plugins.NuGet;
using FluentStore.Services;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FluentStore.SDK.Plugins.Sources
{
    public class NuGetPluginHandler(IPasswordVaultService passwordVaultService, PluginLoader pluginLoader) : PackageHandlerBase(passwordVaultService)
    {
        public const string NAMESPACE_NUGETPLUGIN = "nuget-plugin";

        private readonly PluginLoader _pluginLoader = pluginLoader;
        private readonly SourceCacheContext _cache = new();

        public override HashSet<string> HandledNamespaces => [NAMESPACE_NUGETPLUGIN];

        public override string DisplayName => "NuGet Plugins";

        internal PluginLoader PluginLoader => _pluginLoader;

        public override ImageBase GetImage()
        {
            return new FileImage("https://upload.wikimedia.org/wikipedia/commons/thumb/2/25/NuGet_project_logo.svg/240px-NuGet_project_logo.svg.png")
            {
                ImageType = ImageType.Logo
            };
        }

        public override async IAsyncEnumerable<PackageBase> GetFeaturedPackagesAsync()
        {
            var searchResource = await PluginLoader.FluentStoreRepo.GetResourceAsync<PackageSearchResource>();
            if (searchResource is null)
                yield break;

            var searchResults = await searchResource.SearchAsync(string.Empty, new SearchFilter(true), 0, 15, NullLogger.Instance, default);
            foreach (var result in searchResults)
            {
                yield return new NuGetPluginPackage(this, result)
                {
                    Status = PackageStatus.BasicDetails
                };
            }
        }

        public override async Task<PackageBase> GetPackage(Urn packageUrn, PackageStatus targetStatus = PackageStatus.Details)
        {
            var findPackageResource = await PluginLoader.FluentStoreRepo.GetResourceAsync<FindPackageByIdResource>();
            if (findPackageResource is null)
                return null;

            var packageId = packageUrn.GetContent();

            var allVersions = await findPackageResource.GetAllVersionsAsync(packageId, _cache, NullLogger.Instance, default);
            var latestVersion = FluentStoreNuGetProject.SupportedSdkRange.FindBestMatch(allVersions);
            if (latestVersion is null)
                return null;

            MemoryStream nupkgStream = new();
            await findPackageResource.CopyNupkgToStreamAsync(packageId, latestVersion, nupkgStream, _cache, NullLogger.Instance, default);

            PackageArchiveReader nupkgReader = new(nupkgStream);
            var nuspec = await nupkgReader.GetNuspecReaderAsync(default);
            return new NuGetPluginPackage(this, nuspec: nuspec, reader: nupkgReader)
            {
                Status = PackageStatus.Details
            };
        }

        public override Task<PackageBase> GetPackageFromUrl(Url url) => Task.FromResult<PackageBase>(null);

        public override Url GetUrlFromPackage(PackageBase package) => null;
    }
}

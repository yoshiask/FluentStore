using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetRun.Models;

namespace WinGetRun
{
    public class WinGetApi
    {
        public async Task<PaginatedResponse> GetPublisherPackages(string publisherId, PaginationOptions pageOptions = default)
        {
            var result = await Constants.WINGETRUN_API_HOST.AppendPathSegments("packages", publisherId)
                .SetPaginationOptions(pageOptions).GetJsonAsync<PaginatedResponse>();
            return result;
        }

        public async Task<Package> GetPackage(string publisherId, string packageId)
        {
            var result = await Constants.WINGETRUN_API_HOST.AppendPathSegments("packages", publisherId, packageId)
                .GetJsonAsync<JObject>();
            return result["Package"].ToObject<Package>();
        }

        public async Task<List<Package>> GetFeatured()
        {
            var result = await Constants.WINGETRUN_API_HOST.AppendPathSegments("featured")
                .GetJsonAsync<JObject>();
            return result["Packages"].ToObject<List<Package>>();
        }

        public async Task<Package> GetPackage(string combinedId)
        {
            (string publisherId, string packageId) = Package.GetPublisherAndPackageIds(combinedId);
            return await GetPackage(publisherId, packageId);
        }

        public async Task<PaginatedResponse> SearchPackages(string query, SearchOptions searchOptions = default, PaginationOptions pageOptions = default)
        {
            var result = await Constants.WINGETRUN_API_HOST.AppendPathSegment("packages")
                .SetSearchOptions(searchOptions).SetPaginationOptions(pageOptions).SetQueryParam("query", query)
                .GetJsonAsync<PaginatedResponse>();
            return result;
        }

        public async Task<PaginatedResponse> SearchPackages(string name = null, string publisher = null, string description = null, string tags = null,
            SearchOptions searchOptions = default, PaginationOptions pageOptions = default)
        {
            var request = Constants.WINGETRUN_API_HOST.AppendPathSegment("packages")
                .SetSearchOptions(searchOptions).SetPaginationOptions(pageOptions);

            if (name != null)
                request = request.SetQueryParam("name", name);
            if (publisher != null)
                request = request.SetQueryParam("publisher", publisher);
            if (description != null)
                request = request.SetQueryParam("description", description);
            if (tags != null)
                request = request.SetQueryParam("tags", tags);

            var result = await request.GetJsonAsync<PaginatedResponse>();
            return result;
        }

        public async Task<PaginatedResponse> SearchPackages(string name = null, string publisher = null, string description = null, IEnumerable<string> tags = null,
            SearchOptions searchOptions = default, PaginationOptions pageOptions = default)
        {
            return await SearchPackages(name, publisher, description, string.Join(",", tags), searchOptions, pageOptions);
        }

        public async Task<Manifest> GetManifest(string packageId, string packageVersion)
        {
            var result = await Constants.WINGETRUN_API_HOST.AppendPathSegments("manifests", packageId, packageVersion)
                .GetJsonAsync<JObject>();
            return result["Manifest"].ToObject<Manifest>();
        }
    }
}

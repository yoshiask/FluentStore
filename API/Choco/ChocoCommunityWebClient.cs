using Chocolatey.Models;
using Flurl;
using Flurl.Http.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Versioning;

namespace Chocolatey;

public class ChocoCommunityWebClient : IChocoSearchService
{
    public async Task<IReadOnlyList<Package>> SearchAsync(string query, string targetFramework = "", bool includePrerelease = false, int top = 30, int skip = 0)
    {
        var doc = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment("Search()")
            .SetQueryParam("$filter", "IsLatestVersion").SetQueryParam("$top", top).SetQueryParam("$skip", skip)
            .SetQueryParam("searchTerm", "'" + query + "'")
            .SetQueryParam("targetFramework", "'" + targetFramework + "'")
            .SetQueryParam("includePrerelease", includePrerelease.ToLowerString())
            .GetXDocumentAsync();
        IEnumerable<XElement> entries = doc.Root.Elements().Where(elem => elem.Name.LocalName == "entry");

        // Parse Atom entries into Package model
        return entries.Select(entry => new Package(entry)).ToList();
    }

    public async Task<Package> GetPackageAsync(string id, NuGetVersion version)
    {
        var entry = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment($"Packages(Id='{id}',Version='{version}')")
            .GetXDocumentAsync();
        return new Package(entry.Root);
    }

    public async Task<string> GetPackagePropertyAsync(string id, NuGetVersion version, string propertyName)
    {
        var entry = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment($"Packages(Id='{id}',Version='{version}')")
            .AppendPathSegment(propertyName)
            .GetXDocumentAsync();
        return entry.Root.Value;
    }
}

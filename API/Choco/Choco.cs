using Atom.Xml;
using Flurl;
using Flurl.Http;
using Flurl.Http.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chocolatey
{
    public static class Choco
    {
        public static async Task<List<Entry>> SearchAsync(string query, string targetFramework = "", bool includePrerelease = false, int top = 30, int skip = 0)
        {
            var doc = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment("Search()")
                .SetQueryParam("$filter", "IsLatestVersion").SetQueryParam("$top", top).SetQueryParam("$skip", skip)
                .SetQueryParam("searchTerm", "'" + query + "'")
                .SetQueryParam("targetFramework", "'" + targetFramework + "'")
                .SetQueryParam("includePrerelease", includePrerelease.ToLowerString())
                .GetXDocumentAsync();
            return doc.Root.Elements().Where(elem => elem.Name.LocalName == "entry")
                .Select(entry => entry.Deserialize<Entry>()).ToList();
        }
    }
}

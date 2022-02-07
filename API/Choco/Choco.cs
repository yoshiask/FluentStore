using Chocolatey.Models;
using Flurl;
using Flurl.Http;
using Flurl.Http.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Chocolatey
{
    public static class Choco
    {
        /// <summary>
        /// A method that parses a <see langword="string"/> into the specified <typeparamref name="T"/>.
        /// </summary>
        public delegate T Parse<T>(string str);

        public static async Task<IReadOnlyList<Package>> SearchAsync(string query, string targetFramework = "", bool includePrerelease = false, int top = 30, int skip = 0)
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

        public static async Task<Package> GetPackageAsync(string id, Version version)
        {
            var entry = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment($"Packages(Id='{id}',Version='{version}')")
                .GetXDocumentAsync();
            return new Package(entry.Root);
        }

        public static async Task<string> GetPackagePropertyAsync(string id, Version version, string propertyName)
        {
            var entry = await Constants.CHOCOLATEY_API_HOST.AppendPathSegment($"Packages(Id='{id}', Version='{version}')")
                .AppendPathSegment(propertyName)
                .GetXDocumentAsync();
            return entry.Root.Value;
        }

        public static async Task<T> GetPackagePropertyAsync<T>(string id, Version version, string propertyName, Parse<T> parse)
        {
            return parse(await GetPackagePropertyAsync(id, version, propertyName));
        }

        #region GetPackage*PropertyAsync

        public static async Task<DateTimeOffset> GetPackageDatePropertyAsync(string id, Version version, string propertyName)
        {
            return await GetPackagePropertyAsync(id, version, propertyName, DateTimeOffset.Parse);
        }
        public static async Task<bool> GetPackageBooleanPropertyAsync(string id, Version version, string propertyName)
        {
            return await GetPackagePropertyAsync(id, version, propertyName, bool.Parse);
        }
        public static async Task<int> GetPackageInt32PropertyAsync(string id, Version version, string propertyName)
        {
            return await GetPackagePropertyAsync(id, version, propertyName, int.Parse);
        }
        public static async Task<long> GetPackageInt64PropertyAsync(string id, Version version, string propertyName)
        {
            return await GetPackagePropertyAsync(id, version, propertyName, long.Parse);
        }

        #endregion
    }
}

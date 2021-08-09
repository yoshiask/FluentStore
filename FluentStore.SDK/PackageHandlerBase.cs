using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public abstract class PackageHandlerBase
    {
        public abstract HashSet<string> HandledNamespaces { get; }

        public abstract Task<List<PackageBase>> SearchAsync(string query);

        public abstract Task<List<PackageBase>> GetSearchSuggestionsAsync(string query);

        public abstract Task<PackageBase> GetPackage(Urn packageUrn);
    }
}

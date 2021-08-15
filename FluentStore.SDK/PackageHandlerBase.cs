using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.SDK
{
    public abstract class PackageHandlerBase : IEqualityComparer<PackageHandlerBase>
    {
        public abstract HashSet<string> HandledNamespaces { get; }

        public abstract Task<List<PackageBase>> SearchAsync(string query);

        public abstract Task<List<PackageBase>> GetSearchSuggestionsAsync(string query);

        public abstract Task<PackageBase> GetPackage(Urn packageUrn);

        public bool Equals(PackageHandlerBase x, PackageHandlerBase y) => x.GetType() == y.GetType();

        public int GetHashCode(PackageHandlerBase obj) => obj.GetType().GetHashCode();
    }
}

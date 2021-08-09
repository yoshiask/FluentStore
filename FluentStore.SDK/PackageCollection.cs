using Garfoot.Utilities.FluentUrn;
using System.Collections.Generic;
using System.Linq;

namespace FluentStore.SDK
{
    /// <summary>
    /// Represents a collection of packages from multiple sources,
    /// grouped together by common package IDs.
    /// </summary>
    /// <remarks>
    /// Internally, dictionary where the key is the common package ID, and the value
    /// is a list of <see cref="PackageBase"/> from various handlers.
    /// </remarks>
    public class PackageCollection : Dictionary<Urn, List<PackageBase>>
    {
        public List<Urn> PackageIds => Keys.ToList();

        /// <summary>
        /// Collapses multiple packages with identical IDs into a single <see cref="PackageBase"/>.
        /// </summary>
        public IEnumerable<PackageBase> Collapse()
        {
            // TODO: Come up with an algorithm to merge better
            foreach (var entry in Values)
            {
                yield return entry[0];
            }
        }

        /// <summary>
        /// Determines whether the <see cref="PackageCollection"/> contains the specified package.
        /// </summary>
        /// <param name="packageUrn">The package to locate in the <see cref="PackageCollection"/>.</param>
        /// <returns>
        /// true if the <see cref="PackageCollection"/> contains a package with the specified ID;
        /// otherwise, false.
        /// </returns>
        /// <remarks>
        /// Identical to <see cref="Dictionary{TKey, TValue}.ContainsKey(TKey)"/>.
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool ContainsPackage(Urn packageUrn) => ContainsKey(packageUrn);
    }
}

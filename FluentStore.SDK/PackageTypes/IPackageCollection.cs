using System.Collections.ObjectModel;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Represents a collection.
    /// </summary>
    public interface IPackageCollection
    {
        /// <summary>
        /// The list of packages in this collection.
        /// </summary>
        public ObservableCollection<PackageBase> Items { get; set; }
    }
}

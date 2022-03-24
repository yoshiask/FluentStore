using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Represents a package collection where packages can be added and removed.
    /// </summary>
    public interface IEditablePackageCollection
    {
        /// <summary>
        /// Whether this collection can be edited.
        /// </summary>
        public bool CanEditItems { get; set; }

        /// <summary>
        /// Adds a package to the collection.
        /// </summary>
        /// <param name="package">
        /// The package to add to the collection.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The collection cannot be edited.
        /// </exception>
        public void Add(PackageBase package);

        /// <summary>
        /// Removes the first occurrence of a specific package from the collection.
        /// </summary>
        /// <param name="package">
        /// The package to remove.
        /// </param>
        /// <exception cref="System.NotSupportedException">
        /// The collection cannot be edited.
        /// </exception>
        public void Remove(PackageBase package);

        /// <summary>
        /// Commits the changes.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// The collection cannot be edited.
        /// </exception>
        public Task SaveAsync();
    }
}

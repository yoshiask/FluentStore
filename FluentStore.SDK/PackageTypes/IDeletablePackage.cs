using System;
using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Represents a package that can be deleted.
    /// </summary>
    public interface IDeletablePackage
    {
        /// <summary>
        /// Whether this package can be deleted.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Deletes the package.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The package cannot be deleted.
        /// </exception>
        public Task DeleteAsync();
    }
}

using System.Threading.Tasks;

namespace FluentStore.SDK.Packages
{
    /// <summary>
    /// Represents a package whose details can be edited.
    /// </summary>
    public interface IEditablePackage
    {
        /// <summary>
        /// Whether this package can be edited.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Creates a new <see cref="AbstractUI.Models.AbstractForm"/> representing a form
        /// that can edit this package.
        /// </summary>
        public AbstractUI.Models.AbstractForm CreateEditForm();

        /// <summary>
        /// Commits the changes.
        /// </summary>
        /// <exception cref="System.NotSupportedException">
        /// The package cannot be edited.
        /// </exception>
        public Task SaveAsync();
    }
}

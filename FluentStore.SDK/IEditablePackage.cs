using FluentStore.SDK.AbstractUI.Models;
using OwlCore.AbstractUI.Models;

namespace FluentStore.SDK
{
    public interface IEditablePackage
    {
        public bool IsEditable { get; protected set; }

        /// <inheritdoc cref="CreateEditForm"/>
        public AbstractForm EditForm { get; protected set; }

        /// <summary>
        /// Gets the <see cref="AbstractUICollection"/> that represents a form to edit this package.
        /// </summary>
        /// <remarks>
        /// This collection should not include cancel or submit buttons.
        /// </remarks>
        protected AbstractForm CreateEditForm();
    }
}

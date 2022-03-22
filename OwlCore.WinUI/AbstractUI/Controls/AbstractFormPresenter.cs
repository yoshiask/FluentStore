using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.AbstractUI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using OwlCore.AbstractUI.ViewModels;

namespace OwlCore.WinUI.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractForm"/>.
    /// </summary>
    public partial class AbstractFormPresenter : Control
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbstractFormPresenter"/>.
        /// </summary>
        public AbstractFormPresenter()
        {
            this.DefaultStyleKey = typeof(AbstractFormPresenter);
        }

        internal static AbstractUICollectionViewModel ConvertToCollectionVm(AbstractFormViewModel formVm)
        {
            return new((AbstractForm)formVm.Model);
        }
    }
}

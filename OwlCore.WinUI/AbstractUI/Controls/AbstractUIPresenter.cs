using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;

namespace OwlCore.WinUI.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays any <see cref="AbstractUIElement"/>.
    /// </summary>
    public partial class AbstractUIPresenter : Control
    {
        /// <summary>
        /// Backing property for <see cref="ViewModel"/>.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(AbstractUIViewModelBase), typeof(AbstractUIPresenter), new PropertyMetadata(null));

        /// <summary>
        /// Backing property for <see cref="TemplateSelector"/>.
        /// </summary>
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(AbstractUIPresenter), new PropertyMetadata(null));

        /// <summary>
        /// The ViewModel for this UserControl.
        /// </summary>
        public AbstractUIViewModelBase? ViewModel
        {
            get => (AbstractUIViewModelBase)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// The template selector used to display Abstract UI elements. Use this to define your own custom styles for each control. You may specify the existing, default styles for those you don't want to override.
        /// </summary>
        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }

        /// <summary>
        /// Creates a new instance of <see cref="AbstractUIPresenter"/>.
        /// </summary>
        public AbstractUIPresenter()
        {
            this.DefaultStyleKey = typeof(AbstractUIPresenter);
        }
    }
}

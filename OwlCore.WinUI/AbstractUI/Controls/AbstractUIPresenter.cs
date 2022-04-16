using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.AbstractUI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System;

namespace OwlCore.WinUI.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays any <see cref="AbstractUIElement"/>.
    /// </summary>
    public partial class AbstractUIPresenter : Control
    {
        private bool _changingValue = false;

        /// <summary>
        /// Backing property for <see cref="ViewModel"/>.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(AbstractUIViewModelBase), typeof(AbstractUIPresenter), new PropertyMetadata(null, ViewModel_PropertyChanged));

        /// <summary>
        /// Backing property for <see cref="Model"/>.
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(nameof(Model), typeof(AbstractUIBase), typeof(AbstractUIPresenter), new PropertyMetadata(null, Model_PropertyChanged));

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
        /// The model for this UserControl.
        /// </summary>
        public AbstractUIBase? Model
        {
            get => (AbstractUIBase)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        /// <summary>
        /// The template selector used to display Abstract UI elements.
        /// Use this to define your own custom styles for each control.
        /// You may specify the existing, default styles for those you don't want to override.
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

        /// <summary>
        /// Creates a new instance of <see cref="AbstractUIPresenter"/> to display the
        /// provided <paramref name="viewModel"/>.
        /// </summary>
        public AbstractUIPresenter(AbstractUIViewModelBase viewModel) : this()
        {
            ViewModel = viewModel;
        }

        /// <summary>
        /// Creates an <see cref="AbstractUIViewModelBase"/> from an <see cref="AbstractUIBase"/>.
        /// </summary>
        public static AbstractUIViewModelBase CreateViewModel(AbstractUIBase model)
        {
            if (model == null)
                return null;
            return model switch
            {
                AbstractTextBox textBox => new AbstractTextBoxViewModel(textBox),
                AbstractDataList dataList => new AbstractDataListViewModel(dataList),
                AbstractButton button => new AbstractButtonViewModel(button),
                AbstractBoolean boolean => new AbstractBooleanViewModel(boolean),
                AbstractRichTextBlock richText => new AbstractRichTextBlockViewModel(richText),
                AbstractMultiChoice multiChoiceUIElement => new AbstractMultiChoiceViewModel(multiChoiceUIElement),
                AbstractForm form => new AbstractFormViewModel(form),
                AbstractUICollection elementGroup => new AbstractUICollectionViewModel(elementGroup),
                AbstractProgressIndicator progress => new AbstractProgressIndicatorViewModel(progress),
                AbstractColorPicker color => new AbstractColorPickerViewModel(color),

                _ => throw new NotSupportedException($"No match ViewModel was found for {model.GetType()}."),
            };
        }

        private static void Model_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AbstractUIPresenter p || p._changingValue)
                return;

            p._changingValue = true;
            p.ViewModel = CreateViewModel(e.NewValue as AbstractUIBase);
            p._changingValue = false;
        }

        private static void ViewModel_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AbstractUIPresenter p || p._changingValue)
                return;

            p._changingValue = true;
            p.Model = (e.NewValue as AbstractUIViewModelBase)?.Model;
            p._changingValue = false;
        }
    }
}

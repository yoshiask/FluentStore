using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace FluentStore.Controls
{
    [ContentProperty(Name = nameof(SettingsActionableElement))]
    public sealed partial class SettingsDisplayControl : UserControl
    {
        public FrameworkElement SettingsActionableElement { get; set; }

        public FrameworkElement AdditionalDescriptionContent
        {
            get => (FrameworkElement)GetValue(AdditionalDescriptionContentProperty);
            set => SetValue(AdditionalDescriptionContentProperty, value);
        }
        public static readonly DependencyProperty AdditionalDescriptionContentProperty = DependencyProperty.Register(
            nameof(AdditionalDescriptionContent), typeof(FrameworkElement), typeof(SettingsDisplayControl), new PropertyMetadata(null));

        public object Title
        {
            get => (object)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
              nameof(Title), typeof(object), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register(
              nameof(TitleTemplate), typeof(DataTemplate), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(SettingsDisplayControl), new PropertyMetadata(null));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(IconElement), typeof(SettingsDisplayControl), new PropertyMetadata(null));

        public SettingsDisplayControl()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, "NormalState", false);
        }

        private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width == e.PreviousSize.Width || ActionableElement == null)
                return;

            if (ActionableElement.ActualWidth > e.NewSize.Width / 3)
            {
                VisualStateManager.GoToState(this, "CompactState", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "NormalState", false);
            }
        }
    }
}

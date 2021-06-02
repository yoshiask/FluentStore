using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace FluentStore.Controls
{
    public sealed class IconButton : Button
    {
        public IconButton()
        {
            DefaultStyleKey = typeof(IconButton);
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(IconButton), new PropertyMetadata(null));
    }
}

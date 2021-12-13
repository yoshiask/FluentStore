using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public sealed partial class PackageCardControl : UserControl
    {
        public PackageCardControl()
        {
            this.InitializeComponent();
        }

        public PackageViewModel ViewModel
        {
            get => (PackageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(PackageViewModel), typeof(PackageCardControl), new PropertyMetadata(null));
    }
}

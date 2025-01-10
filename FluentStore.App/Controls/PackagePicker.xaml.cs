using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Controls
{
    public sealed partial class PackagePicker : UserControl
    {
        public PackagePicker()
        {
            this.InitializeComponent();
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(object), typeof(PackagePicker), new PropertyMetadata(null));

        public SelectionMode SelectionMode
        {
            get => (SelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            nameof(SelectionMode), typeof(SelectionMode), typeof(PackagePicker), new PropertyMetadata(SelectionMode.Multiple));

        public PackageManagerViewModel ViewModel
        {
            get => (PackageManagerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(PackageManagerViewModel), typeof(PackagePicker), new PropertyMetadata(null));

        private async void PackageListView_ItemClick(object sender, ItemClickEventArgs e) =>
            await ViewModel.ViewCommand.ExecuteAsync(e.ClickedItem as PluginPackageBase);

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var package = (sender as FrameworkElement)?.DataContext as PluginPackageBase;
            if (package is null)
                return;

            ViewModel.SelectedPackages.Add(package);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var package = (sender as FrameworkElement)?.DataContext as PluginPackageBase;
            if (package is null)
                return;

            ViewModel.SelectedPackages.Remove(package);
        }
    }
}

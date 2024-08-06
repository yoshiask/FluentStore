using FluentStore.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FluentStore.Controls
{
    public sealed partial class PackageCardGridView : UserControl
    {
        public PackageCardGridView()
        {
            this.InitializeComponent();
            Packages = new ObservableCollection<PackageViewModel>();
        }

        public ObservableCollection<PackageViewModel> Packages
        {
            get => (ObservableCollection<PackageViewModel>)GetValue(PackagesProperty);
            set => SetValue(PackagesProperty, value);
        }
        public static readonly DependencyProperty PackagesProperty = DependencyProperty.Register(
            nameof(Packages), typeof(ObservableCollection<PackageViewModel>), typeof(PackageCardGridView), new PropertyMetadata(null));

        public PackageViewModel SelectedPackage
        {
            get => (PackageViewModel)GetValue(SelectedPackageProperty);
            set => SetValue(SelectedPackageProperty, value);
        }
        public static readonly DependencyProperty SelectedPackageProperty =
            DependencyProperty.Register(nameof(SelectedPackage), typeof(PackageViewModel), typeof(PackageCardGridView), new PropertyMetadata(null));

        public int MaxRows
        {
            get => (int)GetValue(MaxRowsProperty);
            set => SetValue(MaxRowsProperty, value);
        }
        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register(
            nameof(MaxRows), typeof(int), typeof(PackageCardGridView), new PropertyMetadata(-1));

        public ICommand ViewPackageCommand
        {
            get => (ICommand)GetValue(ViewPackageCommandProperty);
            set => SetValue(ViewPackageCommandProperty, value);
        }
        public static readonly DependencyProperty ViewPackageCommandProperty = DependencyProperty.Register(
            nameof(ViewPackageCommand), typeof(ICommand), typeof(PackageCardGridView), new PropertyMetadata(null));

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not PackageViewModel pvm)
                return;

            if (ViewPackageCommand != null && ViewPackageCommand.CanExecute(null))
                ViewPackageCommand.Execute(null);
            else if (pvm.ViewPackageCommand != null && pvm.ViewPackageCommand.CanExecute(null))
                await pvm.ViewPackageCommand.ExecuteAsync(null);
            else
                await pvm.ViewPackage();
        }
    }
}

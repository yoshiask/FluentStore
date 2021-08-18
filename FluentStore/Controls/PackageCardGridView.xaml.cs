using FluentStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 0)
            {
                var pvm = (PackageViewModel)e.AddedItems[0];
                pvm.ViewPackage(pvm);
            }
        }
    }
}

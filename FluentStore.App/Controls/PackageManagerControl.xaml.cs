using FluentStore.SDK.Plugins.Sources;
using FluentStore.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace FluentStore.Controls;

public sealed partial class PackageManagerControl : UserControl
{
    public PackageManagerControl()
    {
        this.InitializeComponent();
    }

    public PackageManagerViewModel ViewModel { get; } = new();

    private void PackageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var package in e.RemovedItems.Cast<PluginPackageBase>())
            ViewModel.SelectedPackages.Remove(package);

        foreach (var package in e.AddedItems.Cast<PluginPackageBase>())
            ViewModel.SelectedPackages.Add(package);
    }

    private void PackageListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.PackageToView = e.ClickedItem as PluginPackageBase;
    }
}

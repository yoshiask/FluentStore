using FluentStore.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        foreach (var package in e.RemovedItems.Cast<PackageViewModel>())
            ViewModel.SelectedPackages.Remove(package);

        foreach (var package in e.AddedItems.Cast<PackageViewModel>())
            ViewModel.SelectedPackages.Add(package);

    }

    private void PackageListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        ViewModel.PackageToView = e.ClickedItem as PackageViewModel;
    }
}

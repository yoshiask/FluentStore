using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using FluentStore.SDK.Plugins.Sources;
using FluentStore.SDK;
using FluentStore.ViewModels;
using OwlCore.ComponentModel;
using System.Collections.Specialized;

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

        private void PackageListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var package in e.RemovedItems.Cast<PluginPackageBase>())
                ViewModel.SelectedPackages.Remove(package);

            foreach (var package in e.AddedItems.Cast<PluginPackageBase>())
                ViewModel.SelectedPackages.Add(package);
        }

        private async void PackageListView_ItemClick(object sender, ItemClickEventArgs e) =>
            await ViewModel.ViewCommand.ExecuteAsync(e.ClickedItem as PluginPackageBase);
    }
}

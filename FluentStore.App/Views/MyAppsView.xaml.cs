using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Management.Deployment;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyAppsView : ViewBase
    {
        public MyAppsView()
        {
            this.InitializeComponent();
            ViewModel = new MyAppsViewModel();
        }

        public MyAppsViewModel ViewModel
        {
            get => (MyAppsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(MyAppsViewModel), typeof(MyAppsView), new PropertyMetadata(null));

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
            FilterButton.IsEnabled = false;

            PackageManager pkgManager = new();

            IEnumerable<Package> packages = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041)
                && ApiInformation.IsMethodPresent(nameof(PackageManager), nameof(PackageManager.FindProvisionedPackages))
                ? pkgManager.FindProvisionedPackages()
                : pkgManager.FindPackagesForUser(string.Empty);

            List<AppViewModel> apps = new();

            await System.Threading.Tasks.Parallel.ForEachAsync(packages, async (pkg, token) =>
            {
                var entry = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();

                AppViewModel app = entry != null
                    ? new(pkg, entry, pkg.Id.FamilyName)
                    : new(pkg);

                apps.Add(app);

                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await app.LoadIconSourceCommand.ExecuteAsync(null);
                });
            });

            ViewModel.InitAppsCollection(apps);

            FilterButton.IsEnabled = true;
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        private void FilterItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleMenuFlyoutItem item || item.Tag is not int options)
                return;

            var currentFilter = ViewModel.CurrentFilter;
            if (item.IsChecked)
                currentFilter |= (MyAppsFilterOptions)options;
            else
                currentFilter &= (MyAppsFilterOptions)(options ^ byte.MaxValue);

            ViewModel.ApplyFilter(currentFilter);
        }
    }
}

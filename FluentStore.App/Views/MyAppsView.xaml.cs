using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class MyAppsView : Page
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
            ViewModel.Apps = new ObservableCollection<AppViewModelBase>();
            PackageManager pkgManager = new();

            var appsList = new ObservableCollection<AppViewModelBase>();
            IEnumerable<Package> packages = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041)
                && ApiInformation.IsMethodPresent(nameof(PackageManager), nameof(PackageManager.FindProvisionedPackages))
                ? pkgManager.FindProvisionedPackages()
                : pkgManager.FindPackagesForUser(string.Empty);

            foreach (var pkg in packages.OrderBy(p => p.DisplayName))
            {
                var entry = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();

                AppViewModel app = entry != null
                    ? new(pkg, entry, pkg.Id.FamilyName)
                    : new(pkg);

                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    await app.LoadIconSourceCommand.ExecuteAsync(null);

                    ViewModel.Apps.Add(app);
                });
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }
    }
}

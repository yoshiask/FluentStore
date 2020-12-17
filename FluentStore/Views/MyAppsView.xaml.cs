using FluentStore.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Management.Deployment;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            ViewModel.IsLoadingMyApps = true;
            ViewModel.Apps = new ObservableCollection<AppViewModelBase>();
            PackageManager pkgManager = new PackageManager();

            var appsList = new ObservableCollection<AppViewModelBase>();
            foreach (var pkg in pkgManager.FindPackagesForUser("").OrderBy(p => p.DisplayName))
            {
                var entry = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();
                if (entry != null)
                {
                    var app = new AppViewModel(entry, pkg.Id.FamilyName);
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await app.LoadIconSourceAsync();

                        appsList.Add(app);
                    });
                }
            }
            ViewModel.IsLoadingMyApps = false;
            ViewModel.Apps = appsList;
        }
    }
}

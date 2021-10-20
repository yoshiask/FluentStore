using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Management.Deployment;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
            foreach (var pkg in pkgManager.FindPackagesForUser(string.Empty).OrderBy(p => p.DisplayName))
            {
                var entry = (await pkg.GetAppListEntriesAsync()).FirstOrDefault();
                if (entry != null)
                {
                    var app = new AppViewModel(entry, pkg.Id.FamilyName);
                    Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await app.LoadIconSourceAsync();

                        ViewModel.Apps.Add(app);
                    });
                }
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }
    }
}

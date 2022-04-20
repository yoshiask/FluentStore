using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.Helpers;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        public SettingsView()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Settings"));
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await Settings.Default.SaveAsync();
            base.OnNavigatingFrom(e);
        }

        private void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadCache cache = new(createIfDoesNotExist: false);
            cache.Clear();
        }

        private async void BugReportButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("https://github.com/yoshiask/FluentStore/issues/new");
        }

        private async void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("http://josh.askharoun.com/donate");
        }

        private void CrashButton_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            throw new System.Exception("An unhandled exception was thrown. The app should have crashed and pushed a notification " +
                "that allows the user to view and report the error.");
#endif
        }

        private void PackageHandlerEnable_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleSwitch ts) return;

            Settings.Default.SetPackageHandlerEnabledState(ts.DataContext.GetType().Name, ts.IsOn);
        }

        private void OpenPluginDirButton_Click(object sender, RoutedEventArgs e)
        {
            // Add a trailing slash to ensure that Explorer opens the folder,
            // and not a file that might have the same name
            System.Diagnostics.Process.Start("explorer.exe", $"\"{Settings.Default.PluginDirectory}\"{System.IO.Path.DirectorySeparatorChar}");
        }
    }
}

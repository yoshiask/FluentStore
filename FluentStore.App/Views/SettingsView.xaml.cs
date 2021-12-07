using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Windows.ApplicationModel;
using Microsoft.UI.Xaml.Controls;

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
		private readonly ISettingsService Settings = Ioc.Default.GetRequiredService<ISettingsService>();

		private bool IsDebug
        {
			get
            {
#if DEBUG
				return true;
#else
				return false;
#endif
			}
        }

		private string VersionString
        {
            get
            {
				PackageVersion ver = Package.Current.Id.Version;
				return $"{ver.Major}.{ver.Minor}.{ver.Build}";
			}
        }

		public SettingsView()
		{
			this.InitializeComponent();

			WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Settings"));
		}

        private async void BugReportButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
			await NavigationService.OpenInBrowser("https://github.com/yoshiask/FluentStore/issues/new");
        }

        private async void DonateButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
			await NavigationService.OpenInBrowser("http://josh.askharoun.com/donate");
		}

        private void CrashButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
#if DEBUG
			throw new System.Exception("An unhandled exception was thrown. The app should have crashed and pushed a notification " +
				"that allows the user to view and report the error.");
#endif
		}
    }
}

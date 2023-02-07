using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Settings
{
    public sealed partial class Info : UserControl
    {
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        public Info()
        {
            this.InitializeComponent();
        }

        private void OpenLogDirButton_Click(object sender, RoutedEventArgs e)
        {
            ICommonPathManager commonPathManager = Ioc.Default.GetRequiredService<ICommonPathManager>();

            // Add a trailing slash to ensure that Explorer opens the folder,
            // and not a file that might have the same name
            System.Diagnostics.Process.Start("explorer.exe", $"\"{commonPathManager.GetDefaultLogDirectory()}{System.IO.Path.DirectorySeparatorChar}\"");
        }

        private async void SendFeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("https://github.com/yoshiask/FluentStore/issues/new/choose");
        }

        private async void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.OpenInBrowser("http://josh.askharoun.com/donate");
        }
    }
}

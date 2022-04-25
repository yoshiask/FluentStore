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
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page, IAppContent
    {
        private readonly INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

        public bool IsCompact { get; private set; }

        public SettingsView()
        {
            this.InitializeComponent();

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Settings"));
        }

        public void OnNavigatedFrom() => Helpers.Settings.Default.SaveAsync();

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.AppNavigateBack();
        }
    }
}

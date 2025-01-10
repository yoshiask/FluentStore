using Microsoft.UI.Xaml;
using FluentStore.Services;
using FluentStore.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Oobe
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IpfsClient : WizardPageBase
    {
        private readonly INavigationService _navigationService;
        private readonly ISettingsService _settings;

        public IpfsClient(StartupWizardViewModel wizard,
            INavigationService navigationService, ISettingsService settings) : base(wizard)
        {
            _navigationService = navigationService;
            _settings = settings;

            this.InitializeComponent();
        }

        private async void ViewDocsButton_Click(object sender, RoutedEventArgs e)
        {
            await _navigationService.OpenInBrowser("https://docs.ipfs.tech/concepts/what-is-ipfs/");
        }
    }
}

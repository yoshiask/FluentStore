using CommunityToolkit.Mvvm.Messaging;
using FluentStore.ViewModels.Auth;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentStore.Views.Auth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountsView : Page
    {
        public AccountsView()
        {
            this.InitializeComponent();
            ViewModel = new AccountsViewModel();
        }

        public AccountsViewModel ViewModel
        {
            get => (AccountsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(AccountsViewModel), typeof(AccountsView), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Accounts"));

            if (e.Parameter is Flurl.Url authCallbackUrl)
            {
                await ViewModel.HandleAuthActivation(authCallbackUrl);
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            ViewModel.LoadAccountHandlers();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Unload();
        }
    }
}

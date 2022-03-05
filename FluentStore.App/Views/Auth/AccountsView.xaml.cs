using FluentStore.ViewModels.Auth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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

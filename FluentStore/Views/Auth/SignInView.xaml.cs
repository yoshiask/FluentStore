using FluentStore.ViewModels.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views.Auth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInView : Page
    {
        public SignInView()
        {
            this.InitializeComponent();
        }

        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Attempt to sign the user in
                ViewModel.SignInCommand.Execute(null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage(string.Empty));
        }
    }
}

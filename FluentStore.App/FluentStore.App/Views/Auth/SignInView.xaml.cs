using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

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

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Attempt to sign the user in
                await ViewModel.SignInCommand.ExecuteAsync(null);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage(string.Empty));
        }

        private void InfoButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            InfoTeachingTip.IsOpen = true;
        }
    }
}

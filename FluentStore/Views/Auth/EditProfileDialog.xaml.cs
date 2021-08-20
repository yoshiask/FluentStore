using FluentStoreAPI.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentStore.Views.Auth
{
    public sealed partial class EditProfileDialog : ContentDialog
    {
        public Profile Profile { get; internal set; }

        public EditProfileDialog(Profile profile)
        {
            this.InitializeComponent();
            Profile = profile;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayNameBox.Text = Profile.DisplayName ?? string.Empty;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Profile.DisplayName = DisplayNameBox.Text;
        }
    }
}

using FluentStoreAPI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Views.Auth
{
    public sealed partial class EditProfileDialog : ContentDialog
    {
        public Profile Profile { get; internal set; }

        public EditProfileDialog(Profile profile, XamlRoot root)
        {
            this.InitializeComponent();
            Profile = profile;
            XamlRoot = root;
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

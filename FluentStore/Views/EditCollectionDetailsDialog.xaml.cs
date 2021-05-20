using FluentStoreAPI.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    public sealed partial class EditCollectionDetailsDialog : ContentDialog
    {
        public Collection Collection { get; internal set; }

        public EditCollectionDetailsDialog(Collection collection)
        {
            this.InitializeComponent();
            Collection = collection;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            NameBox.Text = Collection.Name ?? string.Empty;
            TileGlyphBox.Text = Collection.TileGlyph ?? string.Empty;
            DescriptionBox.Text = Collection.Description ?? string.Empty;
            IsPublicSwitch.IsOn = Collection.IsPublic;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Collection.Name = NameBox.Text;
            Collection.TileGlyph = TileGlyphBox.Text;
            Collection.Description = DescriptionBox.Text;
            Collection.IsPublic = IsPublicSwitch.IsOn;
        }
    }
}

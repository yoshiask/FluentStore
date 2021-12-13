using FluentStoreAPI.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    public sealed partial class EditCollectionDetailsDialog : ContentDialog
    {
        public Collection Collection { get; internal set; }

        public EditCollectionDetailsDialog(Collection collection, XamlRoot root)
        {
            InitializeComponent();
            Collection = collection;
            XamlRoot = root;
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            NameBox.Text = Collection.Name ?? string.Empty;
            ImageUrlBox.Text = Collection.ImageUrl ?? string.Empty;
            TileGlyphBox.Text = Collection.TileGlyph ?? string.Empty;
            DescriptionBox.Text = Collection.Description ?? string.Empty;
            IsPublicSwitch.IsOn = Collection.IsPublic;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Collection.Name = NameBox.Text;
            Collection.ImageUrl = ImageUrlBox.Text;
            Collection.TileGlyph = TileGlyphBox.Text;
            Collection.Description = DescriptionBox.Text;
            Collection.IsPublic = IsPublicSwitch.IsOn;
        }
    }
}

using MicrosoftStore.Models;
using System;
using System.Globalization;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductDetailsView : Page
    {
        public ProductDetailsView()
        {
            this.InitializeComponent();

            Window.Current.SetTitleBar(TitlebarDragTarget);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ProductDetails details)
            {
                ViewModel.Product = new Models.ObservableProductDetails(details);
            }

            BackButton.IsEnabled = this.Frame.CanGoBack;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            OnBackRequested();
        }

        // Handles system-level BackRequested events and page-level back button Click events
        private bool OnBackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            OnBackRequested();
            args.Handled = true;
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;
            var culture = CultureInfo.CurrentUICulture;
            string productId = ViewModel.Product.ProductId;

            var dialog = new ProgressDialog()
            {
                Title = ViewModel.Product.Title,
                Body = "Fetching packages..."
            };
            dialog.ShowAsync();

            var packs = await AdGuard.AdGuardApi.GetFilesFromProductId(
                productId, culture.Name
            );

            dialog.Hide();
            if (packs != null)// && packs.Count > 0)
            {
                var package = Utils.GetLatestDesktopPackage(packs, ViewModel.Product.GetRaw());
                if (package == null)
				{
                    var noPackagesDialog = new ContentDialog()
                    {
                        Title = ViewModel.Product.Title,
                        Content = "No available packages for this product.",
                        PrimaryButtonText = "Ok"
                    };
                    await noPackagesDialog.ShowAsync();
                    return;
                }
                else
				{
                    await Utils.InstallPackage(package, ViewModel.Product.GetRaw());
                }
            }

            InstallButton.IsEnabled = true;
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = new TextBlock()
                {
                    Text = ViewModel.Product.Title,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold
                },
                Content = new ScrollViewer()
                {
                    Content = new TextBlock()
                    {
                        Text = ViewModel.Product.Description,
                        TextWrapping = TextWrapping.Wrap
                    }
                },
                PrimaryButtonText = "Close",
                IsSecondaryButtonEnabled = false
            };
            await dialog.ShowAsync();
            return;
        }
    }
}

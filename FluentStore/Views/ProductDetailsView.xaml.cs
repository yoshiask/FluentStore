using FluentStore.Helpers;
using MicrosoftStore.Models;
using StoreLib.Models;
using StoreLib.Services;
using System;
using System.Globalization;
using System.Linq;
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
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ProductDetails details)
            {
                ViewModel.Product = details;
            }
            else if (e.Parameter is ViewModels.ProductDetailsViewModel vm)
            {
                ViewModel = vm;
            }

            if (ViewModel?.Product != null)
            {
                string packageFamily = ViewModel.Product.PackageFamilyNames[0];
                if (await PackageHelper.IsAppInstalledAsync(packageFamily))
                {
                    UpdateInstallButtonToLaunch();
                }
            }
        }

        private async void InstallButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
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

            DisplayCatalogHandler dcathandler = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(culture, true));
            await dcathandler.QueryDCATAsync(productId);
            var packs = await dcathandler.GetMainPackagesForProductAsync();
            string packageFamilyName = dcathandler.ProductListing.Product.Properties.PackageFamilyName;

            dialog.Hide();
            if (packs != null)// && packs.Count > 0)
            {
                var package = PackageHelper.GetLatestDesktopPackage(packs.ToList(), packageFamilyName, ViewModel.Product);
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
                    if (await PackageHelper.InstallPackage(package, ViewModel.Product))
                    {
                        UpdateInstallButtonToLaunch();
                    }
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

        private void AddToCollection_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Download_Click(object sender, RoutedEventArgs e)
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

            DisplayCatalogHandler dcathandler = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(culture, true));
            await dcathandler.QueryDCATAsync(productId);
            var packs = await dcathandler.GetMainPackagesForProductAsync();
            string packageFamilyName = dcathandler.ProductListing.Product.Properties.PackageFamilyName;

            dialog.Hide();
            if (packs != null)// && packs.Count > 0)
            {
                var package = PackageHelper.GetLatestDesktopPackage(packs.ToList(), packageFamilyName, ViewModel.Product);
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
                    var file = (await PackageHelper.DownloadPackage(package, ViewModel.Product)).Item1;

                    var toast = PackageHelper.GenerateDownloadSuccessToast(package, ViewModel.Product, file);
                    Windows.UI.Notifications.ToastNotificationManager.GetDefault().CreateToastNotifier().Show(toast);

                    var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                    savePicker.SuggestedStartLocation =
                        Windows.Storage.Pickers.PickerLocationId.Downloads;
                    if (file.FileType.EndsWith("bundle"))
                        savePicker.FileTypeChoices.Add("Windows App Bundle", new string[] { file.FileType });
                    else
                        savePicker.FileTypeChoices.Add("Windows App Package", new string[] { file.FileType });
                    savePicker.SuggestedFileName = file.DisplayName;
                    savePicker.SuggestedSaveFile = file;
                    var userFile = await savePicker.PickSaveFileAsync();
                    if (userFile != null)
                    {
                        await file.MoveAndReplaceAsync(userFile);
                    }
                }
            }

            InstallButton.IsEnabled = true;
        }

        private async void InstallUsingAppInstaller_Click(object sender, RoutedEventArgs e)
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

            DisplayCatalogHandler dcathandler = new DisplayCatalogHandler(DCatEndpoint.Production, new Locale(culture, true));
            await dcathandler.QueryDCATAsync(productId);
            var packs = await dcathandler.GetMainPackagesForProductAsync();
            string packageFamilyName = dcathandler.ProductListing.Product.Properties.PackageFamilyName;

            dialog.Hide();
            if (packs != null)// && packs.Count > 0)
            {
                var package = PackageHelper.GetLatestDesktopPackage(packs.ToList(), packageFamilyName, ViewModel.Product);
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
                    await PackageHelper.InstallPackage(package, ViewModel.Product, true);
                }
            }

            InstallButton.IsEnabled = true;
        }

        private void HeroImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            UpdateHeroImageSpacer();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateHeroImageSpacer();
        }

        private void UpdateHeroImageSpacer()
        {
            if (HeroImage.Source is Windows.UI.Xaml.Media.Imaging.BitmapImage bitmap && bitmap.UriSource.Host == "via.placeholder.com")
            {
                HeroImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                double offset = InfoCard.ActualHeight * 1.5
                    + InfoCard.Margin.Top + InfoCard.Margin.Bottom;
                HeroImageSpacer.Height = Math.Max(HeroImage.ActualHeight - offset, 0);
            }
        }

        private void UpdateInstallButtonToLaunch()
        {
            string packageFamily = ViewModel.Product.PackageFamilyNames[0];
            InstallButtonText.Text = "Launch";
            InstallUsingAppInstallerMenuItem.IsEnabled = false;
            InstallButton.Click -= InstallButton_Click;
            InstallButton.Click += async (SplitButton sender, SplitButtonClickEventArgs e) =>
            {
                var app = await PackageHelper.GetAppByPackageFamilyNameAsync(packageFamily);
                await app.LaunchAsync();
            };
        }
    }
}

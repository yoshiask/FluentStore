using FluentStore.Helpers;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using MicrosoftStore.Models;
using StoreLib.Models;
using StoreLib.Services;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
            InitializeComponent();
            ViewModel = new ProductDetailsViewModel();
        }

        public ProductDetailsViewModel ViewModel
        {
            get => (ProductDetailsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(ProductDetailsViewModel), typeof(ProductDetailsView), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ProductDetails details)
            {
                ViewModel.Product = details;
            }
            else if (e.Parameter is ProductDetailsViewModel vm)
            {
                ViewModel = vm;
            }

            if (ViewModel?.Product != null)
            {
                WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Apps"));

                string packageFamily = ViewModel.Product.PackageFamilyNames[0];
                if (await PackageHelper.IsAppInstalledAsync(packageFamily))
                {
                    UpdateInstallButtonToLaunch();
                }
            }
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

        private async void InstallButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            await HandleInstall(false);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;

            ProgressDialog progressDialog = new ProgressDialog()
            {
                Title = ViewModel.Product.Title,
                Body = "Fetching packages..."
            };
            SetUpPackageHelperCallbacks(progressDialog);
            PackageHelper.PackageDownloadedCallback = async (product, details, file) =>
            {
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
            };
            progressDialog.ShowAsync();

            await PackageHelper.DownloadPackage(ViewModel.Product);
            
            progressDialog.Hide();
            InstallButton.IsEnabled = true;
        }

        private async void InstallUsingAppInstaller_Click(object sender, RoutedEventArgs e)
        {
            await HandleInstall(true);
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
                // Height of the card including padding and spacing
                double cardHeight = InfoCard.ActualHeight + InfoCard.Margin.Top + InfoCard.Margin.Bottom
                    + ((StackPanel)ContentScroller.Content).Spacing * 2;

                // Required amount of additional spacing to place the card at the bottom of the hero image,
                // or at the bottom of the page (whichever places the card higher up)
                double offset = Math.Min(HeroImage.ActualHeight - cardHeight, ActualHeight - cardHeight);
                HeroImageSpacer.Height = Math.Max(offset, 0);
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

        public static ProgressDialog SetUpPackageHelperCallbacks(ProgressDialog progressDialog)
        {
            PackageHelper.GettingPackagesCallback = product =>
            {
                progressDialog.Body = "Fetching packages...";
            };
            PackageHelper.NoPackagesCallback = async product =>
            {
                progressDialog.Hide();
                var noPackagesDialog = new ContentDialog()
                {
                    Title = product.Title,
                    Content = "No available packages for this product.",
                    PrimaryButtonText = "Ok"
                };
                await noPackagesDialog.ShowAsync();
            };
            PackageHelper.PackageDownloadingCallback = (product, package) =>
            {
                progressDialog.Body = "Downloading package...";
            };
            PackageHelper.PackageDownloadProgressCallback = async (product, package, downloaded, total) =>
            {
                double prog = (double)downloaded / total;
                await progressDialog.SetProgressAsync(prog);
            };
            PackageHelper.PackageInstallingCallback = async (product, package) =>
            {
                progressDialog.IsIndeterminate = true;
                progressDialog.Body = "Installing package...";
            };
            PackageHelper.PackageInstalledCallback = (product, package) => progressDialog.Hide();

            return progressDialog;
        }

        public async Task HandleInstall(bool? useAppInstaller = null)
        {
            InstallButton.IsEnabled = false;

            ProgressDialog progressDialog = new ProgressDialog()
            {
                Title = ViewModel.Product.Title,
                Body = "Fetching packages..."
            };
            SetUpPackageHelperCallbacks(progressDialog);
            PackageHelper.PackageInstalledCallback = (product, package) => UpdateInstallButtonToLaunch();
            progressDialog.ShowAsync();

            await PackageHelper.InstallPackage(ViewModel.Product, useAppInstaller ?? Settings.Default.UseAppInstaller);

            progressDialog.Hide();
            InstallButton.IsEnabled = true;
        }
    }
}

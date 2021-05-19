using FluentStore.Helpers;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using StoreLib.Models;
using StoreLib.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CollectionView : Page
    {
        public CollectionView()
        {
            this.InitializeComponent();
        }
        public CollectionViewModel ViewModel
        {
            get => (CollectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(CollectionViewModel), typeof(CollectionView), new PropertyMetadata(new CollectionViewModel()));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Collection collection)
            {
                ViewModel.Collection = collection;
            }
            else if (e.Parameter is CollectionViewModel vm)
            {
                ViewModel = vm;
            }

            if (ViewModel?.Collection != null)
            {
                
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = new TextBlock()
                {
                    Text = ViewModel.Collection.Name,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold
                },
                Content = new ScrollViewer()
                {
                    Content = new TextBlock()
                    {
                        Text = ViewModel.Collection.Description,
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

            var progressDialog = new ProgressDialog()
            {
                Title = ViewModel.Collection.Name,
                Body = "Fetching packages..."
            };
            PackageHelper.GettingPackagesCallback = product => progressDialog.ShowAsync();
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
            PackageHelper.PackageDownloadProgressCallback = async (product, package, downloaded, total) =>
            {
                await progressDialog.SetProgressAsync(downloaded / total);
            };
            PackageHelper.PackagesLoadedCallback = product => progressDialog.Hide();
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

            await PackageHelper.DownloadPackage(ViewModel.Items[0].Product);

            InstallButton.IsEnabled = true;
        }

        private async void InstallUsingAppInstaller_Click(object sender, RoutedEventArgs e)
        {
            await HandleInstall(true);
        }

        public async Task HandleInstall(bool? useAppInstaller = null)
        {
            InstallButton.IsEnabled = false;

            var installingDialog = new ProgressDialog()
            {
                Title = ViewModel.Collection.Name,
                Body = "Getting ready..."
            };
            installingDialog.ShowAsync();

            int completedSteps = 0;
            int totalSteps = ViewModel.Items.Count * 3;
            foreach (ProductDetailsViewModel pdvm in ViewModel.Items)
            {
                MicrosoftStore.Models.ProductDetails product = pdvm.Product;

                // Set callbacks
                PackageHelper.GettingPackagesCallback = prod =>
                {
                    installingDialog.Body = prod.ShortTitle + "\r\nFetching packages...";
                };
                PackageHelper.PackageDownloadingCallback = (prod, pkg) =>
                {
                    installingDialog.Body = prod.ShortTitle + "\r\nDownloading package...";
                    installingDialog.Progress = ++completedSteps / totalSteps;
                };
                PackageHelper.PackageInstallingCallback = (prod, pkg) =>
                {
                    installingDialog.Body = prod.ShortTitle + "\r\nInstalling package...";
                    installingDialog.Progress = ++completedSteps / totalSteps;
                };
                PackageHelper.PackageInstalledCallback = (prod, pkg) =>
                {
                    installingDialog.Body = "Installed " + prod.ShortTitle + " version " + pkg.Version.ToString();
                    installingDialog.Progress = ++completedSteps / totalSteps;
                };

                // Install package
                await PackageHelper.InstallPackage(product, useAppInstaller ?? Settings.Default.UseAppInstaller);
            }

            InstallButton.IsEnabled = true;
        }
    }
}

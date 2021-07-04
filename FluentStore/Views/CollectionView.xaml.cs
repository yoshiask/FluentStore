using FluentStore.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
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
    [RequiresSignIn]
    public sealed partial class CollectionView : Page
    {
        public CollectionView()
        {
            InitializeComponent();
            ViewModel = new CollectionViewModel();
        }
        public CollectionViewModel ViewModel
        {
            get => (CollectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(CollectionViewModel), typeof(CollectionView), new PropertyMetadata(null));

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
                WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("My Collections"));
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
            WeakReferenceMessenger.Default.Register<PackageFetchStartedMessage>(this, (r, m) =>
            {
                progressDialog.Body = "Fetching packages...";
            });
            WeakReferenceMessenger.Default.Register<PackageFetchFailedMessage>(this, async (r, m) =>
            {
                progressDialog.Hide();
                var noPackagesDialog = new ContentDialog()
                {
                    Title = m.Package.Title,
                    Content = "Failed to fetch packages for this product.",
                    PrimaryButtonText = "Ok"
                };
                await noPackagesDialog.ShowAsync();
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadStartedMessage>(this, (r, m) =>
            {
                progressDialog.Body = "Downloading package...";
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadProgressMessage>(this, async (r, m) =>
            {
                double prog = m.Downloaded / m.Total;
                await progressDialog.SetProgressAsync(prog);
            });
            WeakReferenceMessenger.Default.Register<PackageInstallProgressMessage>(this, async (r, m) =>
            {
                progressDialog.IsIndeterminate = true;
                progressDialog.Body = "Installing package...";
            });
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                progressDialog.Hide();
            });
            WeakReferenceMessenger.Default.Unregister<PackageDownloadCompletedMessage>(this);
            WeakReferenceMessenger.Default.Register<PackageDownloadCompletedMessage>(this, async (r, m) =>
            {
                var file = m.InstallerFile;
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
            });
            progressDialog.ShowAsync();

            //await PackageHelper.DownloadPackage(ViewModel.Items[0].Package);
            await ViewModel.Items[0].Package.DownloadPackageAsync();

            progressDialog.Hide();
            InstallButton.IsEnabled = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);
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

            int totalSteps = ViewModel.Items.Count * 3;
            foreach (PackageViewModel pvm in ViewModel.Items)
            {
                //MicrosoftStore.Models.ProductDetails product = pdvm.Package;

                // Set callbacks
                RegisterPackageServiceMessages(installingDialog, totalSteps);

                // Install package
                if (await pvm.Package.DownloadPackageAsync())
                    await pvm.Package.InstallAsync();
                //await PackageHelper.InstallPackage(product, useAppInstaller ?? Settings.Default.UseAppInstaller);
            }

            InstallButton.IsEnabled = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        public void RegisterPackageServiceMessages(ProgressDialog progressDialog, int totalSteps)
        {
            int completedSteps = 0;
            WeakReferenceMessenger.Default.Register<PackageFetchStartedMessage>(this, (r, m) =>
            {
                progressDialog.Body = m.Package.ShortTitle + "\r\nFetching packages...";
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadStartedMessage>(this, (r, m) =>
            {
                progressDialog.Body = m.Package.ShortTitle + "\r\nDownloading package...";
                progressDialog.Progress = ++completedSteps / totalSteps;
            });
            WeakReferenceMessenger.Default.Register<PackageInstallStartedMessage>(this, (r, m) =>
            {
                progressDialog.Body = m.Package.ShortTitle + "\r\nInstalling package...";
                progressDialog.Progress = ++completedSteps / totalSteps;
            });
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                progressDialog.Body = "Installed " + m.Package.ShortTitle + " version " + m.Package.Version.ToString();
                progressDialog.Progress = ++completedSteps / totalSteps;
            });
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editDialog = new EditCollectionDetailsDialog(ViewModel.Collection);
            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // User wants to save
                await ViewModel.UpdateCollectionAsync(editDialog.Collection);
            }
        }
    }
}

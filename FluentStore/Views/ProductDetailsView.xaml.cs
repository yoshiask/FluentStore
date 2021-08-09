using FluentStore.Helpers;
using FluentStore.SDK;
using FluentStore.SDK.Messages;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PackageView : Page
    {
        public PackageView()
        {
            InitializeComponent();
            ViewModel = new PackageViewModel();
        }

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        public PackageViewModel ViewModel
        {
            get => (PackageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(PackageViewModel), typeof(PackageView), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is PackageBase package)
            {
                ViewModel.Package = package;
            }
            else if (e.Parameter is PackageViewModel vm)
            {
                ViewModel = vm;
            }

            if (ViewModel?.Package != null)
            {
                WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Apps"));

                bool isInstalled = false;
                try
                {
                   isInstalled = await ViewModel.Package.IsPackageInstalledAsync();
                }
                catch (Exception ex)
                {
                    // TODO: Log exception
                }
                if (isInstalled)
                    UpdateInstallButtonToLaunch();
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = new TextBlock()
                {
                    Text = ViewModel.Package.Title,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold
                },
                Content = new ScrollViewer()
                {
                    Content = new TextBlock()
                    {
                        Text = ViewModel.Package.Description,
                        TextWrapping = TextWrapping.Wrap
                    }
                },
                PrimaryButtonText = "Close",
                IsSecondaryButtonEnabled = false
            };
            await dialog.ShowAsync();
            return;
        }

        private async void AddToCollection_Click(object sender, RoutedEventArgs e)
        {
            var FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
            var userService = Ioc.Default.GetRequiredService<UserService>();
            FlyoutBase flyout;
            if (!userService.IsLoggedIn)
            {
                flyout = new Flyout
                {
                    Content = new TextBlock
                    {
                        Text = "Please create an account or\r\nlog in to access this feature.",
                        TextWrapping = TextWrapping.Wrap
                    },
                    Placement = FlyoutPlacementMode.Bottom
                };
            }
            else
            {
                try
                {
                    string userId = userService.CurrentFirebaseUser.LocalID;
                    flyout = new MenuFlyout
                    {
                        Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
                    };
                    foreach (FluentStoreAPI.Models.Collection collection in await FSApi.GetCollectionsAsync(userId))
                    {
                        var item = new MenuFlyoutItem
                        {
                            Text = collection.Name,
                            Tag = collection
                        };
                        item.Click += (object s, RoutedEventArgs e) =>
                        {
                            var it = (MenuFlyoutItem)s;
                            var col = (FluentStoreAPI.Models.Collection)it.Tag;
                            col.Items ??= new System.Collections.Generic.List<string>(1);
                            col.Items.Add(ViewModel.Package.Urn.ToString());
                        };
                        ((MenuFlyout)flyout).Items.Add(item);
                    }
                    flyout.Closed += async (s, e) =>
                    {
                        foreach (var it in ((MenuFlyout)s).Items)
                        {
                            var col = (FluentStoreAPI.Models.Collection)it.Tag;
                            await FSApi.UpdateCollectionAsync(userId, col);
                        }
                    };
                }
                catch
                {
                    flyout = new Flyout
                    {
                        Content = new TextBlock
                        {
                            Text = "An error occurred.",
                            TextWrapping = TextWrapping.Wrap
                        },
                        Placement = FlyoutPlacementMode.Bottom
                    };
                }
            }

            flyout.ShowAt((Button)sender);
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            await HandleInstall(false);
        }

        private async void InstallSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            await HandleInstall(false);
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;

            ProgressDialog progressDialog = new ProgressDialog()
            {
                Title = ViewModel.Package.Title,
                Body = "Fetching packages..."
            };
            RegisterPackageServiceMessages(progressDialog);
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

            await ViewModel.Package.DownloadPackageAsync();

            progressDialog.Hide();
            InstallButton.IsEnabled = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);
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
            InstallUsingAppInstallerMenuItem.IsEnabled = false;

            InstallButtonText.Text = "Launch";
            InstallButton.Click -= InstallSplitButton_Click;
            InstallButton.Click += async (SplitButton sender, SplitButtonClickEventArgs e) =>
            {
                await ViewModel.Package.IsPackageInstalledAsync();
            };
        }

        public ToastNotification RegisterPackageServiceMessages(ProgressDialog progressDialog)
        {
            var progressToast = PackageHelper.GenerateProgressToast(ViewModel.Package);
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

                PackageHelper.HandlePackageDownloadStartedToast(m, progressToast);
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadProgressMessage>(this, async (r, m) =>
            {
                double prog = m.Downloaded / m.Total;
                await progressDialog.SetProgressAsync(prog);

                PackageHelper.HandlePackageDownloadProgressToast(m, progressToast);
            });
            WeakReferenceMessenger.Default.Register<PackageInstallProgressMessage>(this, async (r, m) =>
            {
                progressDialog.IsIndeterminate = true;
                progressDialog.Body = "Installing package...";

                PackageHelper.HandlePackageInstallProgressToast(m, progressToast);
            });
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                progressDialog.Hide();

                PackageHelper.HandlePackageInstallCompletedToast(m, progressToast);
            });

            return progressToast;
        }

        public async Task HandleInstall(bool? useAppInstaller = null)
        {
            InstallButton.IsEnabled = false;

            ProgressDialog progressDialog = new ProgressDialog()
            {
                Title = ViewModel.Package.Title,
                Body = "Fetching packages..."
            };
            var progressToast = RegisterPackageServiceMessages(progressDialog);
            WeakReferenceMessenger.Default.Unregister<PackageInstallCompletedMessage>(this);
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                UpdateInstallButtonToLaunch();
                progressDialog.Hide();

                PackageHelper.HandlePackageInstallCompletedToast(m, progressToast);
            });
            progressDialog.ShowAsync();

            if (await ViewModel.Package.DownloadPackageAsync())
                await ViewModel.Package.InstallAsync();// ViewModel.Package, useAppInstaller ?? Settings.Default.UseAppInstaller);

            progressDialog.Hide();
            InstallButton.IsEnabled = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}

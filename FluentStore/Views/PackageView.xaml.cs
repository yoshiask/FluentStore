﻿using FluentStore.Helpers;
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

namespace FluentStore.Views
{
    public sealed partial class PackageView : Page
    {
        public PackageView()
        {
            InitializeComponent();
            ViewModel = new PackageViewModel();
        }

        FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
        UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();

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
            FlyoutBase flyout;
            if (!UserService.IsLoggedIn)
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
                    string userId = UserService.CurrentFirebaseUser.LocalID;
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
                            Text = "Please create an account or\r\nlog in to access this feature.",
                            TextWrapping = TextWrapping.Wrap
                        },
                        Placement = FlyoutPlacementMode.Bottom
                    };
                }
            }

            flyout.ShowAt((Button)sender);
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

        private void HeroImage_SizeChanged(object sender, RoutedEventArgs e)
        {
            UpdateHeroImageSpacer((FrameworkElement)sender);
        }

        private void UpdateHeroImageSpacer(FrameworkElement imageElem)
        {
            // Height of the card including padding and spacing
            double cardHeight = InfoCard.ActualHeight + InfoCard.Margin.Top + InfoCard.Margin.Bottom
                + ((StackPanel)ContentScroller.Content).Spacing * 2;

            // Required amount of additional spacing to place the card at the bottom of the hero image,
            // or at the bottom of the page (whichever places the card higher up)
            double offset = Math.Min(imageElem.ActualHeight - cardHeight, ActualHeight - cardHeight);
            HeroImageSpacer.Height = Math.Max(offset, 0);
        }

        private void UpdateInstallButtonToLaunch()
        {
            InstallUsingAppInstallerMenuItem.IsEnabled = false;

            InstallButtonText.Text = "Launch";
            InstallButton.Click -= InstallSplitButton_Click;
            InstallButton.Click += async (SplitButton sender, SplitButtonClickEventArgs e) =>
            {
                await ViewModel.Package.LaunchAsync();
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

            if (await ViewModel.Package.DownloadPackageAsync() != null)
                await ViewModel.Package.InstallAsync();// ViewModel.Package, useAppInstaller ?? Settings.Default.UseAppInstaller);

            progressDialog.Hide();
            InstallButton.IsEnabled = true;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        private async void EditCollection_Click(object sender, RoutedEventArgs e)
        {
            FluentStoreAPI.Models.Collection collection = ((SDK.PackageTypes.CollectionPackage)ViewModel.Package).Model;
            var editDialog = new EditCollectionDetailsDialog(collection);

            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                // User wants to save
                await FSApi.UpdateCollectionAsync(UserService.CurrentFirebaseUser.LocalID, editDialog.Collection);
                await ViewModel.Refresh();
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }
        }

        private void DeleteCollection_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase flyout;
            if (!UserService.IsLoggedIn)
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
                var button = new Button
                {
                    Content = "Yes, delete forever",
                };
                flyout = new Flyout
                {
                    Content = new StackPanel
                    {
                        Spacing = 8,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = $"You are about to delete \"{ViewModel.Package.Title}\".\r\nDo you want to continue?",
                                TextWrapping = TextWrapping.Wrap
                            },
                            button
                        }
                    },
                    Placement = FlyoutPlacementMode.Bottom
                };
                button.Click += async (object sender, RoutedEventArgs e) =>
                {
                    string userId = UserService.CurrentFirebaseUser.LocalID;
                    // 0, urn; 1, namespace; 2, userId; 3, collectionId
                    string collectionId = ViewModel.Package.Urn.ToString().Split(':')[3];
                    if (await FSApi.DeleteCollectionAsync(userId, collectionId))
                        NavigationService.NavigateBack();
                };
            }

            flyout.ShowAt((FrameworkElement)sender);
        }
    }
}

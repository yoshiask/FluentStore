using FluentStore.Helpers;
using FluentStore.Helpers.Continuity;
using FluentStore.Helpers.Continuity.Extensions;
using FluentStore.SDK;
using FluentStore.SDK.Helpers;
using FluentStore.SDK.Messages;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.Notifications;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;

namespace FluentStore.Views
{
    public sealed partial class PackageView : Page
    {
        public PackageView()
        {
            InitializeComponent();
            SetUpAnimations();

            ViewModel = new PackageViewModel();
        }

        FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();
        UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
        PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

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
            object param = e.Parameter;

            if (param is PackageBase package)
            {
                ViewModel = new PackageViewModel(package);
            }
            else if (param is PackageViewModel vm)
            {
                ViewModel = vm;
            }
            else if (param is Garfoot.Utilities.FluentUrn.Urn urn)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                try
                {
                    ViewModel = new PackageViewModel(await PackageService.GetPackageAsync(urn));
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(ex);
                }
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }
            else if (param is Flurl.Url url)
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                try
                {
                    ViewModel = new PackageViewModel(await PackageService.GetPackageFromUrlAsync(url));
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(ex);
                }
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }

            if (ViewModel?.Package != null)
            {
                WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Apps"));

                bool canLaunch = false;
                try
                {
                    canLaunch = await ViewModel.Package.CanLaunchAsync();
                }
                catch (Exception ex)
                {
                    var logger = Ioc.Default.GetRequiredService<LoggerService>();
                    logger.UnhandledException(ex, "Exception from Win32 component");
                }
                if (canLaunch)
                    UpdateInstallButtonToLaunch();
            }
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = new TextBlock
                {
                    Text = ViewModel.Package.Title,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold
                },
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = ViewModel.Package.Description,
                        TextWrapping = TextWrapping.Wrap
                    }
                },
                PrimaryButtonText = "Close",
                IsSecondaryButtonEnabled = false
            };
            dialog.XamlRoot = Content.XamlRoot;
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
                    string userId = UserService.CurrentUser.LocalID;
                    var collections = await FSApi.GetCollectionsAsync(userId);
                    if (collections.Count > 0)
                    {
                        flyout = new MenuFlyout
                        {
                            Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
                        };
                        foreach (FluentStoreAPI.Models.Collection collection in collections)
                        {
                            if (ViewModel.Package is SDK.Packages.GenericListPackage<FluentStoreAPI.Models.Collection> curCollection
                                && curCollection.Model.Id == collection.Id)
                            {
                                // ABORT! Do not add to list of options. Attempting to view a collection that contains
                                // itself results in an infinite loop.
                                continue;
                            }

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
                    else
                    {
                        var myCollectionsLink = new Hyperlink
                        {
                            Inlines =
                            {
                                new Run { Text = "My Collections" }
                            },
                        };
                        myCollectionsLink.Click += (sender, args) =>
                        {
                            NavigationService.Navigate(typeof(MyCollectionsView));
                        };
                        var noCollectionsContent = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            Inlines =
                            {
                                new Run { Text = "You don't have any collections." },
                                new LineBreak(),
                                new Run { Text = "Go to " },
                                myCollectionsLink,
                                new Run { Text = " to create one." }
                            }
                        };

                        flyout = new Flyout
                        {
                            Content = noCollectionsContent,
                            Placement = FlyoutPlacementMode.Bottom
                        };
                    }
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    flyout = new Controls.HttpErrorFlyout(ex.StatusCode ?? 418, ex.Message);
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
            InstallButton.IsEnabled = false;

            var progressToast = RegisterPackageServiceMessages();
            WeakReferenceMessenger.Default.Unregister<PackageInstallCompletedMessage>(this);
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, async (r, m) =>
            {
                if (await m.Package.CanLaunchAsync())
                    UpdateInstallButtonToLaunch();
                VisualStateManager.GoToState(this, "NoAction", true);

                PackageHelper.HandlePackageInstallCompletedToast(m, progressToast);
            });
            VisualStateManager.GoToState(this, "Progress", true);

            Flyout flyout = null;
            try
            {
                if (ViewModel.Package.Status.IsLessThan(PackageStatus.Downloaded))
                    await ViewModel.Package.DownloadPackageAsync();
                if (ViewModel.Package.Status.IsAtLeast(PackageStatus.Downloaded))
                {
                    bool installed = await ViewModel.Package.InstallAsync();
                    InstallButton.IsEnabled = true;
                    if (installed)
                    {
                        // Show success
                        flyout = new Flyout
                        {
                            Content = new TextBlock
                            {
                                Text = "Install succeeded!"
                            }
                        };
                    }
                    else
                    {
                        // Show error
                        flyout = new Controls.HttpErrorFlyout(418, "Package was not installed, an unknown error occurred.");
                    }
                }
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                // TODO: Use InfoBar
                flyout = new Controls.HttpErrorFlyout(ex.StatusCode ?? 418, ex.ToString());
            }
            catch (Exception ex)
            {
                // TODO: Use InfoBar
                flyout = new Controls.HttpErrorFlyout(418, ex.ToString());
            }
            finally
            {
                flyout?.ShowAt(InstallButton);
                VisualStateManager.GoToState(this, "NoAction", true);
                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            InstallButton.IsEnabled = false;

            var progressToast = RegisterPackageServiceMessages();
            WeakReferenceMessenger.Default.Unregister<PackageDownloadCompletedMessage>(this);
            WeakReferenceMessenger.Default.Register<PackageDownloadCompletedMessage>(this, async (r, m) =>
            {
                FileInfo file = m.InstallerFile;
                Windows.Storage.Pickers.FileSavePicker savePicker = new()
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
                };

                // Initialize save picker for Win32
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                SDK.Models.InstallerType type = m.Package.Type;
                SDK.Models.InstallerType typeReduced = type.Reduce();
                string extDesc;
                if (typeReduced == SDK.Models.InstallerType.Msix)
                {
                    extDesc = "Windows App " + (type.HasFlag(SDK.Models.InstallerType.Bundle) ? "Bundle" : "Package");

                    if (type.HasFlag(SDK.Models.InstallerType.Encrypted))
                        extDesc = "Encrypted " + extDesc;
                }
                else
                {
                    extDesc = type switch
                    {
                        SDK.Models.InstallerType.Msi => "Windows Installer",
                        SDK.Models.InstallerType.Exe => "Installer",
                        SDK.Models.InstallerType.Zip => "Compressed zip archive",
                        SDK.Models.InstallerType.Inno => "Inno Setup installer",
                        SDK.Models.InstallerType.Nullsoft => "NSIS installer",
                        SDK.Models.InstallerType.Wix => "WiX installer",
                        SDK.Models.InstallerType.Burn => "WiX Burn installer",

                        _ => "Unknown"
                    };
                }
                savePicker.FileTypeChoices.Add(extDesc, new string[] { file.Extension });
                savePicker.SuggestedFileName = file.Name;

                _ = DispatcherQueue.TryEnqueue(() => PackageHelper.HandlePackageDownloadCompletedToast(m, progressToast));

                var userFile = await savePicker.PickSaveFileAsync();
                if (userFile != null)
                {
                    await Task.Run(() => file.MoveTo(userFile.Path, true));
                }
            });
            VisualStateManager.GoToState(this, "Progress", true);

            Flyout flyout = null;
            try
            {
                var storageItem = await ViewModel.Package.DownloadPackageAsync();
                bool downloaded = storageItem != null;
                InstallButton.IsEnabled = true;
                if (downloaded)
                {
                    // Show success
                    flyout = new Flyout
                    {
                        Content = new TextBlock
                        {
                            Text = "Download succeeded!"
                        }
                    };
                }
                else
                {
                    // Show error
                    flyout = new Controls.HttpErrorFlyout(418, "Package was not downloaded, an unknown error occurred.");
                }
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                // TODO: Use InfoBar
                flyout = new Controls.HttpErrorFlyout(ex.StatusCode ?? 418, ex.Message);
            }
            catch (Exception ex)
            {
                // TODO: Use InfoBar
                flyout = new Controls.HttpErrorFlyout(418, ex.Message);
            }
            finally
            {
                if (flyout != null)
                    flyout.ShowAt(InstallButton);
                VisualStateManager.GoToState(this, "NoAction", true);
                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
        }

        private void ShareButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            DataTransferManager dataTransferManager = ShareHelper.GetDataTransferManager(App.Current.Window);
            dataTransferManager.DataRequested += (sender, args) =>
            {
                Flurl.Url appUrl = "fluentstore://package/" + ViewModel.Package.Urn.ToString();
                ShareDataRequested(sender, args, appUrl);
            };
            ShareHelper.ShowShareUIForWindow(App.Current.Window);
        }

        private void ShareWebLink_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = ShareHelper.GetDataTransferManager(App.Current.Window);
            dataTransferManager.DataRequested += (sender, args) =>
            {
                Flurl.Url appUrl = PackageService.GetUrlForPackageAsync(ViewModel.Package);
                ShareDataRequested(sender, args, appUrl);
            };
            ShareHelper.ShowShareUIForWindow(App.Current.Window);
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            Flurl.Url appUrl = PackageService.GetUrlForPackageAsync(ViewModel.Package);
            NavigationService.OpenInBrowser(appUrl);
        }

        private void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args, Flurl.Url appUrl)
        {
            var appUri = appUrl.ToUri();
            DataPackage linkPackage = new DataPackage();
            linkPackage.SetApplicationLink(appUri);

            DataRequest request = args.Request;
            request.Data.SetWebLink(appUri);
            request.Data.Properties.Title = "Share App";
            request.Data.Properties.Description = ViewModel.Package.ShortTitle;
            request.Data.Properties.ContentSourceApplicationLink = appUri;
            if (typeof(SDK.Images.StreamImage).IsAssignableFrom(ViewModel.AppIcon.GetType()))
            {
                var img = (SDK.Images.StreamImage)ViewModel.AppIcon;
            }
        }

        private void HeroImage_SizeChanged(object sender, RoutedEventArgs e)
        {
            UpdateHeroImageSpacer((FrameworkElement)sender);
        }

        private void InfoCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateHeroImageSpacer(HeroImage);
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
            InstallButtonText.Text = "Launch";
            InstallButton.Click -= InstallSplitButton_Click;
            InstallButton.Click += async (SplitButton sender, SplitButtonClickEventArgs e) =>
            {
                await ViewModel.Package.LaunchAsync();
            };
        }

        public ToastNotification RegisterPackageServiceMessages()
        {
            var progressToast = PackageHelper.GenerateProgressToast(ViewModel.Package);

            WeakReferenceMessenger.Default.Register<PackageFetchStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressIndicator.IsIndeterminate = true;
                    ProgressLabel.Text = "Fetching packages...";
                });
            });
            WeakReferenceMessenger.Default.Register<PackageFetchFailedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(async () =>
                {
                    VisualStateManager.GoToState(this, "NoAction", true);
                    var noPackagesDialog = new ContentDialog()
                    {
                        Title = m.Package.Title,
                        Content = "Failed to fetch packages for this product.",
                        PrimaryButtonText = "Ok"
                    };
                    await noPackagesDialog.ShowAsync();
                });
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressLabel.Text = "Downloading package...";

                    PackageHelper.HandlePackageDownloadStartedToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadProgressMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    double prog = m.Downloaded / m.Total;
                    ProgressIndicator.IsIndeterminate = false;
                    ProgressIndicator.Value = prog;
                    ProgressText.Text = $"{prog * 100:##0}%";

                    PackageHelper.HandlePackageDownloadProgressToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageInstallStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressIndicator.IsIndeterminate = true;
                    ProgressText.Text = string.Empty;
                    ProgressLabel.Text = "Installing package...";

                    PackageHelper.HandlePackageInstallProgressToast(new(m.Package, 0), progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageInstallProgressMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    ProgressIndicator.IsIndeterminate = false;
                    ProgressIndicator.Value = m.Progress;
                    ProgressText.Text = $"{m.Progress * 100:##0}%";

                    PackageHelper.HandlePackageInstallProgressToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageInstallCompletedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    VisualStateManager.GoToState(this, "NoAction", true);

                    PackageHelper.HandlePackageInstallCompletedToast(m, progressToast);
                });
            });

            return progressToast;
        }

        private async void EditCollection_Click(object sender, RoutedEventArgs e)
        {
            FluentStoreAPI.Models.Collection collection = ((SDK.Packages.CollectionPackage)ViewModel.Package).Model;
            EditCollectionDetailsDialog editDialog = new(collection, Content.XamlRoot);

            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                    // User wants to save
                    await FSApi.UpdateCollectionAsync(UserService.CurrentUser.LocalID, editDialog.Collection);
                    await ViewModel.Refresh();
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                }
                catch (Flurl.Http.FlurlHttpException ex)
                {
                    new Controls.HttpErrorFlyout(ex.StatusCode ?? 418, ex.Message)
                        .ShowAt(InstallButton);
                }
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
                    string userId = UserService.CurrentUser.LocalID;
                    // 0, urn; 1, namespace; 2, userId; 3, collectionId
                    string collectionId = ViewModel.Package.Urn.ToString().Split(':')[3];
                    try
                    {
                        if (await FSApi.DeleteCollectionAsync(userId, collectionId))
                            NavigationService.NavigateBack();
                    }
                    catch (Flurl.Http.FlurlHttpException ex)
                    {
                        // TODO: Show error message
                    }
                };
            }

            flyout.ShowAt((FrameworkElement)sender);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            string state = (App.Current.Window.Bounds.Width > (double)App.Current.Resources["CompactModeMinWidth"])
                ? "DefaultLayout" : "CompactLayout";
            VisualStateManager.GoToState(this, state, true);
        }

        private void SetUpAnimations()
        {
            var compositor = this.Visual().Compositor;

            // Create background visuals.
            //var infoCardVisual = compositor.CreateSpriteVisual();
            //var infoCardVisualBrush = infoCardVisual.Brush = compositor.CreateBackdropBrush();
            //InfoCard.SetChildVisual(infoCardVisual);

            // Sync background visual dimensions.
            //InfoCard.SizeChanged += (s, e) => infoCardVisual.Size = e.NewSize.ToVector2();

            // Enable implilcit Offset and Size animations.
            var easing = compositor.EaseOutSine();

            IconBox.EnableImplicitAnimation(VisualPropertyType.All, 400, easing: easing);
            TitleBlock.EnableImplicitAnimation(VisualPropertyType.All, 100, easing: easing);
            SubheadBlock.EnableImplicitAnimation(VisualPropertyType.All, 100, easing: easing);
            ActionBar.EnableImplicitAnimation(VisualPropertyType.All, 100, easing: easing);

            // Enable implicit Visible/Collapsed animations.
            ProgressGrid.EnableFluidVisibilityAnimation(axis: AnimationAxis.Y,
                showFromScale: Vector2.UnitX, hideToScale: Vector2.UnitX, showDuration: 400, hideDuration: 250);
        }
    }
}

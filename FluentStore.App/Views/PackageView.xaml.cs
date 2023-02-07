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
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Notifications;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;
using FluentStore.SDK.Models;
using OwlCore.WinUI.AbstractUI.Controls;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;

namespace FluentStore.Views
{
    public sealed partial class PackageView : ViewBase
    {
        public PackageView()
        {
            InitializeComponent();
            SetUpAnimations();

            ViewModel = new PackageViewModel();
        }

        INavigationService NavigationService = Ioc.Default.GetRequiredService<INavigationService>();
        PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        ICommonPathManager PathManager = Ioc.Default.GetRequiredService<ICommonPathManager>();

        public PackageViewModel ViewModel
        {
            get => (PackageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(PackageViewModel), typeof(PackageView), new PropertyMetadata(null));

        public override async void OnNavigatedTo(object param)
        {
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
                catch (WebException ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(ex.StatusCode, ex.Message);
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(418, ex.Message);
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
                catch (WebException ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(ex.StatusCode, ex.Message);
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    NavigationService.ShowHttpErrorPage(418, ex.Message);
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
                    logger.UnhandledException(ex, OwlCore.Diagnostics.LogLevel.Error);
                }
                if (canLaunch)
                    UpdateInstallButtonToLaunch();

                // Show additional commands
                foreach (var button in ViewModel.GetAdditionalCommands())
                {
                    MenuFlyoutItem commandItem = new()
                    {
                        DataContext = button,
                        Text = button.Text,
                        Icon = new FontIcon
                        {
                            Glyph = button.IconCode,
                        }
                    };

                    commandItem.Click += (_, _) => HandleAdditionalCommand(button.ClickCommand);

                    ToolTipService.SetToolTip(commandItem, button.TooltipText);

                    InstallMenu.Items.Add(commandItem);
                }
            }
        }

        private async void AddToCollection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                List<PackageBase> collections = new();
                await foreach (var c in PackageService.GetCollectionsAsync())
                    if (c.PackageHandler.CanEditCollection(c))
                        collections.Add(c);

                if (collections.Count > 0)
                {
                    var flyout = new MenuFlyout
                    {
                        Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft
                    };

                    foreach (var package in collections)
                    {
                        if (ViewModel.Package == package
                            || package is not SDK.Packages.IPackageCollection collection)
                        {
                            // ABORT! Do not add to list of options. Attempting to view a collection that contains
                            // itself results in an infinite loop.
                            continue;
                        }

                        var item = new MenuFlyoutItem
                        {
                            Text = package.Title,
                            Tag = package
                        };
                        item.Click += async (object s, RoutedEventArgs e) =>
                        {
                            // Add package to collection
                            var it = (MenuFlyoutItem)s;
                            var col = (SDK.Packages.IPackageCollection)it.Tag;
                            col.Items.Add(ViewModel.Package);

                            // Save collection
                            var package = (PackageBase)it.Tag;
                            await package.PackageHandler.SavePackageAsync(package);
                        };
                        flyout.Items.Add(item);
                    }

                    flyout.ShowAt((FrameworkElement)sender);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new WarningMessage(
                        "You don't have any collections.\r\nGo to 'Collections' to create one."));
                }
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, ViewModel, ErrorType.PackageSaveFailed));
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        private async void InstallSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs e)
        {
            try
            {
                RegisterPackageServiceMessages();
                ViewModel.IsInstalling = true;

                if (ViewModel.Package.Status.IsLessThan(PackageStatus.Downloaded))
                    await ViewModel.Package.DownloadAsync(PathManager.GetTempDirectory());

                if (ViewModel.Package.Status.IsAtLeast(PackageStatus.Downloaded))
                {
                    bool installed = await ViewModel.Package.InstallAsync();
                    if (installed && await ViewModel.Package.CanLaunchAsync())
                        UpdateInstallButtonToLaunch();
                }
            }
            finally
            {
                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegisterPackageServiceMessages();
                ViewModel.IsInstalling = false;

                await ViewModel.Package.DownloadAsync(PathManager.GetTempDirectory());
            }
            finally
            {
                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
        }

        private void HandleAdditionalCommand(IRelayCommand command)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                try
                {
                    RegisterPackageServiceMessages();
                    ViewModel.IsInstalling = false;

                    command.Execute(null);
                }
                finally
                {
                    WeakReferenceMessenger.Default.UnregisterAll(this);
                }
            });
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

        private async void ShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args, Flurl.Url appUrl)
        {
            var appUri = appUrl.ToUri();
            DataPackage linkPackage = new();
            linkPackage.SetApplicationLink(appUri);

            DataRequest request = args.Request;
            var def = request.GetDeferral();

            request.Data.SetWebLink(appUri);
            request.Data.Properties.Title = "Share App";
            request.Data.Properties.Description = ViewModel.Package.ShortTitle;
            request.Data.Properties.ContentSourceApplicationLink = appUri;
            if (ViewModel.AppIcon is SDK.Images.StreamImage img)
            {
                var stream = await img.GetImageStreamAsync();
                var raStream = stream.AsRandomAccessStream();
                request.Data.Properties.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromStream(raStream);
            }

            def.Complete();
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
            double offsetA = imageElem.ActualHeight - cardHeight;
            double offsetB = ActualHeight - cardHeight;
            double offset;

            if (offsetA > offsetB)
            {
                offset = offsetB;
                ScrollDetailsButton.Visibility = Visibility.Visible;
            }
            else
            {
                offset = offsetA;
                ScrollDetailsButton.Visibility = Visibility.Collapsed;
            }
            HeroImageSpacer.Height = Math.Max(offset, 0);
        }

        private void UpdateInstallButtonToLaunch()
        {
            InstallButtonText.Text = "Launch";
            InstallButton.Click -= InstallSplitButton_Click;
            InstallButton.Click += async (SplitButton sender, SplitButtonClickEventArgs e)
                => await ViewModel.Package.LaunchAsync();
        }

        private void SetProgressUI(bool isInProgress)
        {
            if (isInProgress)
            {
                InstallButton.IsEnabled = false;
                VisualStateManager.GoToState(this, "Progress", true);
            }
            else
            {
                InstallButton.IsEnabled = true;
                VisualStateManager.GoToState(this, "NoAction", true);
            }
        }

        private ToastNotification RegisterPackageServiceMessages()
        {
            var progressToast = PackageHelper.GenerateProgressToast(ViewModel.Package);

            WeakReferenceMessenger.Default.UnregisterAll(this);

            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    if (m.Context is PackageBase p)
                    {
                        SetProgressUI(false);

                        switch (m.Type)
                        {
                            case ErrorType.PackageDownloadFailed:
                                PackageHelper.HandlePackageDownloadFailedToast(m, progressToast);
                                break;

                            case ErrorType.PackageInstallFailed:
                                PackageHelper.HandlePackageInstallFailedToast(m, progressToast);
                                break;
                        }
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<PackageFetchStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    SetProgressUI(true);
                    ProgressIndicator.IsIndeterminate = true;
                    ProgressLabel.Text = "Fetching packages...";
                });
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    SetProgressUI(true);
                    ProgressLabel.Text = "Downloading package...";

                    PackageHelper.HandlePackageDownloadStartedToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageDownloadProgressMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    double prog = 100 * m.Downloaded / m.Total;

                    SetProgressUI(true);
                    ProgressIndicator.IsIndeterminate = false;
                    ProgressIndicator.Value = prog;
                    ProgressText.Text = $"{prog:##0}%";

                    PackageHelper.HandlePackageDownloadProgressToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<PackageInstallStartedMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                {
                    SetProgressUI(true);
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
                    double prog = 100 * m.Progress;

                    SetProgressUI(true);

                    ProgressIndicator.IsIndeterminate = false;
                    ProgressIndicator.Value = prog;
                    ProgressText.Text = $"{prog:##0}%";

                    PackageHelper.HandlePackageInstallProgressToast(m, progressToast);
                });
            });
            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, (r, m) =>
            {
                _ = DispatcherQueue.TryEnqueue(async delegate
                {
                    if (m.Context is PackageBase p)
                    {
                        SetProgressUI(false);

                        switch (m.Type)
                        {
                            case SuccessType.PackageInstallCompleted:
                                PackageHelper.HandlePackageInstallCompletedToast(m, progressToast);
                                break;

                            case SuccessType.PackageDownloadCompleted:
                                // Don't ask the user to save the downloaded item if they
                                // asking to install it automatically.
                                if (!ViewModel.IsInstalling)
                                {
                                    PackageHelper.HandlePackageDownloadCompletedToast(m, progressToast);

                                    if (p.DownloadItem != null && p.Status.IsAtLeast(PackageStatus.Downloaded))
                                    {
                                        if (p.DownloadItem is FileInfo file)
                                        {
                                            // Use file save picker
                                            Windows.Storage.Pickers.FileSavePicker savePicker = new()
                                            {
                                                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
                                            };

                                            // Initialize save picker for Win32
                                            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, NavigationService.GetMainWindowHandle());

                                            savePicker.FileTypeChoices.Add(p.Type.GetExtensionDescription(), new[] { file.Extension });
                                            savePicker.SuggestedFileName = file.Name;

                                            var userFile = await savePicker.PickSaveFileAsync();
                                            if (userFile != null)
                                            {
                                                file.MoveTo(userFile.Path, true);
                                            }
                                        }
                                        else if (p.DownloadItem is DirectoryInfo dir)
                                        {
                                            // Use folder save picker
                                            Windows.Storage.Pickers.FolderPicker savePicker = new()
                                            {
                                                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
                                            };

                                            // Initialize save picker for Win32
                                            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, NavigationService.GetMainWindowHandle());

                                            var userFolder = await savePicker.PickSingleFolderAsync();
                                            if (userFolder != null)
                                            {
                                                // Don't move the directory, just its contents
                                                foreach (FileSystemInfo item in dir.GetFileSystemInfos())
                                                {
                                                    if (item is FileInfo subFile)
                                                        subFile.MoveTo(userFolder.Path, true);
                                                    else if (item is DirectoryInfo subDir)
                                                        subDir.MoveTo(userFolder.Path);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }
                });
            });

            return progressToast;
        }

        private async void EditPackage_Click(object sender, RoutedEventArgs e)
        {
            var editDialog = new AbstractFormDialog(ViewModel.Package.PackageHandler.CreateEditForm(ViewModel), Content.XamlRoot);
            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                try
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                    // User wants to save
                    await ViewModel.Package.PackageHandler.SavePackageAsync(ViewModel);
                    await ViewModel.Refresh();

                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, ViewModel, ErrorType.PackageSaveFailed));
                }
            }
        }

        private void DeletePackage_Click(object sender, RoutedEventArgs e)
        {
            var button = new Button
            {
                Content = "Yes, delete forever",
            };
            var flyout = new Flyout
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
                try
                {
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                    bool deleted = await ViewModel.Package.PackageHandler.DeletePackageAsync(ViewModel);
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));

                    if (deleted)
                        NavigationService.NavigateBack();
                }
                catch (Exception ex)
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, ViewModel, ErrorType.PackageDeleteFailed));
                }
            };

            flyout.ShowAt((FrameworkElement)sender);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IsCompact = App.Current.Window.Bounds.Width > (double)App.Current.Resources["CompactModeMinWidth"];
            string state = IsCompact ? "DefaultLayout" : "CompactLayout";
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

        private void Screenshot_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not SDK.Images.ImageBase img)
                return;

            // Show screenshot view
            ViewModel.SelectedScreenshot = img;
            FindName(nameof(ScreenshotView));
        }

        private void ScreenshotViewCloseButton_Click(object sender, RoutedEventArgs e)
        {
            UnloadObject(ScreenshotView);
        }

        private void ScrollDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            MainDetailsPanel.StartBringIntoView();
        }
    }
}

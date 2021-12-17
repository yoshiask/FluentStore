using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.Helpers;
using FluentStore.Services;
using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using FluentStore.SDK.Messages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FluentStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : UserControl
    {
        private readonly NavigationService NavService = Ioc.Default.GetService<INavigationService>() as NavigationService;
        private readonly UserService UserService = Ioc.Default.GetService<UserService>();
        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetService<FluentStoreAPI.FluentStoreAPI>();
        
        public ShellViewModel ViewModel
        {
            get => (ShellViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(ShellViewModel), typeof(MainPage), new PropertyMetadata(new ShellViewModel()));

        public bool IsCompact { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            PointerPressed += MainPage_PointerPressed;

            MainNav.BackRequested += (s, e) => NavService.NavigateBack();
            MainFrame.Navigated += MainFrame_Navigated;
            NavService.CurrentFrame = MainFrame;

            foreach (PageInfo page in NavService.Pages)
                MainNav.MenuItems.Add(page.GetNavigationViewItem());
            MainNav.SelectedItem = MainNav.MenuItems[0];

            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
            {
                MainInfoBar.Title = m.Exception.Message;
                MainInfoBar.Message = m.Exception.StackTrace;
                MainInfoBar.Severity = InfoBarSeverity.Error;
                MainInfoBar.IsOpen = true;
            });
            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, (r, m) =>
            {
                // Don't show package fetched messages
                if (m.Type == SuccessType.PackageFetchCompleted) return;

                MainInfoBar.Title = m.Message;
                MainInfoBar.Severity = InfoBarSeverity.Success;
                MainInfoBar.IsOpen = true;
            });

            UserService.OnLoginStateChanged += UserService_OnLoginStateChanged;
            UserService.TrySignIn(false);
        }

        private void MainPage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                // Backward
                NavService.NavigateBack();
                e.Handled = true;
            }
            else if (props.IsXButton2Pressed)
            {
                // Forward
                NavService.NavigateForward();
                e.Handled = true;
            }
        }

        private void UserService_OnLoginStateChanged(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                UserButton.Visibility = Visibility.Visible;
                SignInButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                UserButton.Visibility = Visibility.Collapsed;
                SignInButton.Visibility = Visibility.Visible;

                // If the current page requires sign in, navigate away
                if (RequiresSignInAttribute.IsPresent(MainFrame.Content))
                    NavService.Navigate(typeof(Views.HomeView));

                // Remove all pages from the back stack
                // that require authentication
                var backStack = MainFrame.BackStack.ToArray();
                foreach (PageStackEntry entry in backStack)
                {
                    if (RequiresSignInAttribute.IsPresent(entry.SourcePageType))
                        MainFrame.BackStack.Remove(entry);
                }
            }
        }

        private async void controlsSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent() && !string.IsNullOrWhiteSpace(ViewModel.SearchBoxText)
                && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                await ViewModel.GetSearchSuggestionsAsync();
            }
        }

        private async void controlsSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is PackageViewModel vm)
            {
                await ViewModel.SubmitQueryAsync(vm);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                // Switching pages, hacky way to close the suggestions menu
                sender.IsEnabled = false;
                sender.IsEnabled = true;
                NavService.Navigate(typeof(Views.SearchResultsView), args.QueryText);
            }
        }
        
        private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            controlsSearchBox.Focus(FocusState.Programmatic);
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            MainNav.IsBackEnabled = MainFrame.CanGoBack;
            try
            {
                // Update the NavView when the frame navigates on its own.
                // This is in a try-catch block so that I don't have to do a dozen
                // null checks.

                var page = NavService.Pages.Find((info) => info.PageType == e.SourcePageType);
                if (page == null)
                {
                    MainNav.SelectedItem = null;
                    return;
                }

                MainNav.SelectedItem = MainNav.MenuItems.First(obj => (obj as NavigationViewItem)?.Tag == page);
            }
            catch
            {
                MainNav.SelectedItem = null;
            }
        }

        private async void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type page = typeof(Views.HomeView);

            if (args.IsSettingsSelected)
            {
                page = typeof(Views.SettingsView);
                goto navigate;
            }

            if (args.SelectedItem is not NavigationViewItem navItem) goto navigate;

            PageInfo pageInfo = NavService.Pages.Find((info) => info == navItem.Tag);
            if (pageInfo == null) goto navigate;

            page = pageInfo.PageType;
            if (pageInfo.RequiresSignIn)
            {
                await UserService.TrySignIn(true);
                if (!UserService.IsLoggedIn)
                    return;
            }

        navigate:
            NavService.Navigate(page);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Set titlebar to page header when in compact
            double CompactModeMinWidth = (double)App.Current.Resources["CompactModeMinWidth"];
            if (e.NewSize.Width <= CompactModeMinWidth && e.PreviousSize.Width > CompactModeMinWidth)
            {
                VisualStateManager.GoToState(this, "CompactLayout", true);
                IsCompact = true;
            }
            else if (e.NewSize.Width > CompactModeMinWidth && e.PreviousSize.Width <= CompactModeMinWidth)
            {
                VisualStateManager.GoToState(this, "DefaultLayout", true);
                IsCompact = false;
                WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage(string.Empty));
            }
        }

        private async void EditProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var editDialog = new Views.Auth.EditProfileDialog(UserService.CurrentProfile, Content.XamlRoot);

            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                //WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
                // User wants to save
                await FSApi.UpdateUserProfileAsync(UserService.CurrentUser.LocalID, editDialog.Profile);
                //WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }
        }
    }
}

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
    public sealed partial class MainPage : UserControl, IAppContent
    {
        private readonly NavigationService NavService = Ioc.Default.GetService<INavigationService>() as NavigationService;
        
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

            WeakReferenceMessenger.Default.Register<ErrorMessage>(this, ErrorMessage_Recieved);
            WeakReferenceMessenger.Default.Register<WarningMessage>(this, WarningMessage_Recieved);
            WeakReferenceMessenger.Default.Register<SuccessMessage>(this, SuccessMessage_Recieved);
        }

        public void OnNavigatedTo(object parameter) => UpdateNavViewSelected(MainFrame.CurrentPageType);

        private void ErrorMessage_Recieved(object r, ErrorMessage m)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                MainInfoBar.Title = m.Exception.Message;
                MainInfoBar.Message = m.Exception.StackTrace;
                MainInfoBar.Severity = InfoBarSeverity.Error;
                MainInfoBar.IsOpen = true;
            });
        }

        private void WarningMessage_Recieved(object r, WarningMessage m)
        {
            _ = DispatcherQueue.TryEnqueue(delegate
            {
                MainInfoBar.Title = m.Message;
                MainInfoBar.Message = null;
                MainInfoBar.Severity = InfoBarSeverity.Warning;
                MainInfoBar.IsOpen = true;
            });
        }

        private void SuccessMessage_Recieved(object r, SuccessMessage m)
        {
            // Don't show package fetched messages
            if (m.Type == SuccessType.PackageFetchCompleted) return;

            _ = DispatcherQueue.TryEnqueue(delegate
            {
                MainInfoBar.Title = m.Message;
                MainInfoBar.Message = null;
                MainInfoBar.Severity = InfoBarSeverity.Success;
                MainInfoBar.IsOpen = true;
            });
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

        private void UpdateNavViewSelected(Type sourcePageType)
        {
            MainNav.IsBackEnabled = MainFrame.CanGoBack;
            try
            {
                // Update the NavView when the frame navigates on its own.
                // This is in a try-catch block so that I don't have to do a dozen
                // null checks.

                var page = NavService.Pages.Find(info => info.PageType == sourcePageType);
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

        private void MainFrame_Navigated(object sender, object e) => UpdateNavViewSelected(e.GetType());

        private void NavigationView_SelectionChanged(object sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type page = typeof(Views.HomeView);

            if (args.IsSettingsSelected)
            {
                NavService.AppNavigate(typeof(Views.SettingsView), null);
                return;
            }

            if (args.SelectedItem is not NavigationViewItem navItem) goto navigate;

            PageInfo pageInfo = NavService.Pages.Find(navItem.Tag.Equals);
            if (pageInfo == null) goto navigate;

            page = pageInfo.PageType;

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
    }
}

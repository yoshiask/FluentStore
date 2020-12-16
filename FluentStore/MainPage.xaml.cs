using FluentStore.Helpers;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FluentStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Services.NavigationService NavService { get; } = Ioc.Default.GetService<Services.INavigationService>() as Services.NavigationService;

        public MainPage()
        {
            this.InitializeComponent();

            MainFrame.Navigated += MainFrame_Navigated;
            NavigationHelper.PageFrame = MainFrame;
            NavService.CurrentFrame = MainFrame;

            foreach(PageInfo page in NavigationHelper.Pages)
            {
                var item = new Microsoft.UI.Xaml.Controls.NavigationViewItem()
                {
                    Content = page.Title,
                    Icon = page.Icon,
                    Visibility = page.Visibility,
                };
                MainNav.MenuItems.Add(item);
                AutomationProperties.SetName(item, page.Title);
            }
            MainNav.SelectedItem = MainNav.MenuItems[0];
        }

        private async void controlsSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.CheckCurrent() && args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                await ViewModel.GetSearchSuggestionsAsync();
        }

        private async void controlsSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is MicrosoftStore.Models.Product product)
            {
                await ViewModel.SubmitQueryAsync(product);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
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
                var page = NavigationHelper.Pages.Find((info) => info.PageType == e.SourcePageType);
                if (page == null)
                {
                    MainNav.SelectedItem = null;
                    return;
                }
                MainNav.SelectedItem = MainNav.MenuItems.ToList().Find((obj) => (obj as Microsoft.UI.Xaml.Controls.NavigationViewItem).Content.ToString() == page.Title);
            }
            catch
            {
                MainNav.SelectedItem = null;
            }
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavService.Navigate(typeof(Views.SettingsView));
                return;
            }

            if (!(args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem))
            {
                NavService.Navigate(typeof(Views.HomeView));
                return;
            }

            PageInfo pageInfo = NavigationHelper.Pages.Find((info) => info.Title == navItem.Content.ToString());
            if (pageInfo == null)
            {
                NavService.Navigate(typeof(Views.HomeView));
                return;
            }

            if (pageInfo != null && pageInfo.PageType.BaseType == typeof(Page))
                NavService.Navigate(pageInfo.PageType);
        }
	}
}

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
        public ObservableCollection<MicrosoftStore.Models.Product> Results { get; set; }
        MicrosoftStore.Models.ProductDetails CurrentProduct { get; set; } = null;

        public MainPage()
        {
            this.InitializeComponent();

            MainFrame.Navigated += MainFrame_Navigated;
            NavigationHelper.PageFrame = MainFrame;

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
            if (!args.CheckCurrent())
                return;

            var r = await GetSuggestions(sender.Text);
            if (r == null)
                return;
            Results = new ObservableCollection<MicrosoftStore.Models.Product>(r.Where(s => s.Source == "Apps"));

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suggestions = new List<MicrosoftStore.Models.Product>();

                var querySplit = sender.Text.Split(" ");
                var matchingItems = Results.Where(
                    item =>
                    {
                        // Idea: check for every word entered (separated by space) if it is in the name, 
                        // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                        // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                        bool flag = true;
                        foreach (string queryToken in querySplit)
                        {
                            // Check if token is not in string
                            if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                // Token is not in string, so we ignore this item.
                                flag = false;
                            }
                        }
                        return flag;
                    }
                );
                foreach (var item in matchingItems)
                {
                    suggestions.Add(item);
                }
                if (suggestions.Count > 0)
                {
                    controlsSearchBox.ItemsSource = suggestions.OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title);
                }
                else
                {
                    controlsSearchBox.ItemsSource = new string[] { "No results found" };
                }
            }
        }

        private async void controlsSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is MicrosoftStore.Models.Product product)
            {
                try
                {
                    LoadingIndicator.Visibility = Visibility.Visible;

                    var culture = CultureInfo.CurrentUICulture;
                    var region = new RegionInfo(culture.LCID);
                    string productId = product.Metas.First(m => m.Key == "BigCatalogId").Value;

                    // Get the full product details
                    var item = await Ioc.Default.GetRequiredService<MicrosoftStore.IStorefrontApi>().GetProduct(productId, region.TwoLetterISORegionName, culture.Name);
                    var candidate = item.Convert<MicrosoftStore.Models.ProductDetails>().Payload;
                    if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                    {
                        CurrentProduct = candidate;
                        LoadingIndicator.Visibility = Visibility.Collapsed;
                        Frame.Navigate(typeof(Views.ProductDetailsView), CurrentProduct);
                    }
                }
                catch (ArgumentNullException ex)
                {
                    Debug.WriteLine(ex.ParamName + ":\r\n" + ex.StackTrace);
                }
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                //NavigationRootPage.RootFrame.Navigate(typeof(SearchResultsPage), controlsSearchBox.ItemsSource as IEnumerable<MicrosoftStore.Models.Product>);
            }
        }
        
        private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            controlsSearchBox.Focus(FocusState.Programmatic);
        }

        public async Task<List<MicrosoftStore.Models.Product>> GetSuggestions(string query)
        {
            var suggs = await Ioc.Default.GetRequiredService<MicrosoftStore.IMSStoreApi>().GetSuggestions(
                query, "en-US", MicrosoftStore.Constants.CLIENT_ID,
                new string[] { MicrosoftStore.Constants.CAT_ALL_PRODUCTS }, new int[] { 10, 0, 0 }
            );
            if (suggs.ResultSets.Count <= 0)
                return null;
            return suggs.ResultSets[0].Suggests;
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
                NavigationHelper.NavigateToSettings();
                return;
            }

            if (!(args.SelectedItem is Microsoft.UI.Xaml.Controls.NavigationViewItem navItem))
            {
                NavigationHelper.NavigateToHome();
                return;
            }

            PageInfo pageInfo = NavigationHelper.Pages.Find((info) => info.Title == navItem.Content.ToString());
            if (pageInfo == null)
            {
                NavigationHelper.NavigateToHome();
                return;
            }

            if (pageInfo != null && pageInfo.PageType.BaseType == typeof(Page))
                MainFrame.Navigate(pageInfo.PageType);
        }
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FluentStore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<MicrosoftStore.Models.Product> Results { get; set; }
        AdGuard.Models.Package CurrentPackage { get; set; }
        MicrosoftStore.Models.ProductDetails CurrentProduct { get; set; } = null;

        public MainPage()
        {
            this.InitializeComponent();
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
                    var item = await Apis.StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name);
                    var candidate = (item.Payload as Newtonsoft.Json.Linq.JObject).ToObject<MicrosoftStore.Models.ProductDetails>();
                    if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                    {
                        CurrentProduct = candidate;
                        LoadingIndicator.Visibility = Visibility.Collapsed;
                        Frame.Navigate(typeof(Views.ProductDetailsView), CurrentProduct);
                    }

                    //LoadingIndicator.Visibility = Visibility.Visible;

                    //ImageBackPanel.Visibility = Visibility.Collapsed;
                    //TitlePanel.Visibility = Visibility.Collapsed;
                    //InstallButton.Visibility = Visibility.Collapsed;
                    //ImagePanel.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(product.ImageUri);
                    //TitlePanel.Text = product.Title;
                    //ImageBackPanel.Visibility = Visibility.Visible;
                    //TitlePanel.Visibility = Visibility.Visible;

                    //var culture = CultureInfo.CurrentUICulture;
                    //var region = new RegionInfo(culture.LCID);

                    //LinksView.Items.Clear();
                    //string productId = product.Metas.First(m => m.Key == "BigCatalogId").Value;
                    //var packs = await AdGuard.AdGuardApi.GetFilesFromProductId(
                    //    productId, culture.Name
                    //);
                    //if (packs != null)
                    //{
                    //    // Get the full product details
                    //    var page = await Apis.StorefrontApi.GetPage(productId, region.TwoLetterISORegionName, culture.Name, "0.0.0.0");
                    //    foreach (var item in page)
                    //    {
                    //        var candidate = (item.Payload as Newtonsoft.Json.Linq.JObject).ToObject<MicrosoftStore.Models.ProductDetails>();
                    //        if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
                    //        {
                    //            CurrentProduct = candidate;
                    //            break;
                    //        }
                    //    }
                    //    if (CurrentProduct == null)
                    //        return;

                    //    foreach (AdGuard.Models.Package package in packs)
                    //        LinksView.Items.Add(package);
                    //    CurrentPackage = Utils.GetLatestDesktopPackage(packs, CurrentProduct);

                    //    InstallButton.Visibility = Visibility.Visible;
                    //}
                    //else
                    //{
                    //    //LinksView.Items.Add("No packages found");
                    //}
                    //LoadingIndicator.Visibility = Visibility.Collapsed;
                }
                catch (ArgumentNullException ex)
                {
                    Debug.WriteLine(ex.ParamName + ":\r\n" + ex.StackTrace);
                }
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                //NavigationRootPage.RootFrame.Navigate(typeof(SearchResultsPage), args.QueryText);
            }
        }
        
        private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            controlsSearchBox.Focus(FocusState.Programmatic);
        }

        public async Task<List<MicrosoftStore.Models.Product>> GetSuggestions(string query)
        {
            var suggs = await Apis.MicrosoftStoreApi.GetSuggestions(
                query, "en-US", MicrosoftStore.Constants.CLIENT_ID,
                new string[] { MicrosoftStore.Constants.CAT_ALL_PRODUCTS }, new int[] { 10, 0, 0 }
            );
            if (suggs.ResultSets.Count <= 0)
                return null;
            return suggs.ResultSets[0].Suggests;
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            await Utils.InstallPackage(CurrentPackage, CurrentProduct);
        }
    }
}

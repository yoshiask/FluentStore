using FSAPI = FluentStoreAPI.FluentStoreAPI;
using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using MicrosoftStore;
using MicrosoftStore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        public ShellViewModel()
        {
            GetSearchSuggestionsCommand = new AsyncRelayCommand(GetSearchSuggestionsAsync);
            SubmitQueryCommand = new AsyncRelayCommand<Product>(SubmitQueryAsync);
            SignInCommand = new AsyncRelayCommand(SignInAsync);
            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        }

        private readonly IStorefrontApi StorefrontApi = Ioc.Default.GetRequiredService<IStorefrontApi>();
        private readonly IMSStoreApi MSStoreApi = Ioc.Default.GetRequiredService<IMSStoreApi>();
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();
        private readonly IPasswordVaultService PasswordVaultService = Ioc.Default.GetRequiredService<IPasswordVaultService>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();

        private ObservableCollection<Product> _SearchSuggestions = new ObservableCollection<Product>();
        public ObservableCollection<Product> SearchSuggestions
        {
            get => _SearchSuggestions;
            set => SetProperty(ref _SearchSuggestions, value);
        }

        private string _SearchBoxText;
        public string SearchBoxText
        {
            get => _SearchBoxText;
            set => SetProperty(ref _SearchBoxText, value);
        }

        private ProductDetails _SelectedProduct;
        public ProductDetails SelectedProduct
        {
            get => _SelectedProduct;
            set => SetProperty(ref _SelectedProduct, value);
        }

        private object _SearchBoxChosenSuggestion;
        public object SearchBoxChosenSuggestion
        {
            get => _SearchBoxChosenSuggestion;
            set => SetProperty(ref _SearchBoxChosenSuggestion, value);
        }

        private IAsyncRelayCommand _GetSearchSuggestionsCommand;
        public IAsyncRelayCommand GetSearchSuggestionsCommand
        {
            get => _GetSearchSuggestionsCommand;
            set => SetProperty(ref _GetSearchSuggestionsCommand, value);
        }

        private IAsyncRelayCommand<Product> _SubmitQueryCommand;
        public IAsyncRelayCommand<Product> SubmitQueryCommand
        {
            get => _SubmitQueryCommand;
            set => SetProperty(ref _SubmitQueryCommand, value);
        }

        private IAsyncRelayCommand _TestAuthCommand;
        public IAsyncRelayCommand TestAuthCommand
        {
            get => _TestAuthCommand;
            set => SetProperty(ref _TestAuthCommand, value);
        }

        private IAsyncRelayCommand _SignInCommand;
        public IAsyncRelayCommand SignInCommand
        {
            get => _SignInCommand;
            set => SetProperty(ref _SignInCommand, value);
        }

        private IAsyncRelayCommand _SignOutCommand;
        public IAsyncRelayCommand SignOutCommand
        {
            get => _SignOutCommand;
            set => SetProperty(ref _SignOutCommand, value);
        }

        private IAsyncRelayCommand _EditProfileCommand;
        public IAsyncRelayCommand EditProfileCommand
        {
            get => _EditProfileCommand;
            set => SetProperty(ref _EditProfileCommand, value);
        }

        public async Task GetSearchSuggestionsAsync()
        {
            var r = await GetSuggestions(SearchBoxText);
            if (r == null)
                return;
            SearchSuggestions = new ObservableCollection<Product>(r.Where(s => s.Source == "Apps"));

            var suggestions = new List<Product>();

            var querySplit = SearchBoxText.Split(' ');
            var matchingItems = SearchSuggestions.Where(
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
                SearchSuggestions = new ObservableCollection<Product>(
                    suggestions.OrderByDescending(i => i.Title.StartsWith(SearchBoxText, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title)
                );
            }
            else
            {
                SearchSuggestions = new ObservableCollection<Product>(
                    new Product[] { new Product() { Title = "No results found" } }
                );
            }
        }

        public async Task SubmitQueryAsync(Product product)
        {
            var culture = CultureInfo.CurrentUICulture;
            var region = new RegionInfo(culture.LCID);
            string productId = product.Metas.First(m => m.Key == "BigCatalogId").Value;

            // Get the full product details
            var item = await StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name);
            var candidate = item.Convert<ProductDetails>().Payload;
            if (candidate?.PackageFamilyNames != null && candidate?.ProductId != null)
            {
                SelectedProduct = candidate;
                NavService.Navigate("ProductDetailsView", SelectedProduct);
            }
        }

        public async Task<List<Product>> GetSuggestions(string query)
        {
            var suggs = await MSStoreApi.GetSuggestions(
                query, CultureInfo.CurrentUICulture.Name, Constants.CLIENT_ID,
                new string[] { Constants.CAT_ALL_PRODUCTS }, new int[] { 10, 0, 0 }
            );
            if (suggs.ResultSets.Count <= 0)
                return null;
            return suggs.ResultSets[0].Suggests;
        }

        public async Task SignInAsync()
        {
            NavService.Navigate("Auth.SignInView");
        }

        public async Task SignOutAsync()
        {
            FSApi.Token = null;
            FSApi.RefreshToken = null;
            PasswordVaultService.Remove(new CredentialBase());
        }
    }
}

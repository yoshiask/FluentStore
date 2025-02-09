using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class SearchResultsViewModel : ObservableRecipient
    {
        private bool UpdateResultsList = true;

        public SearchResultsViewModel()
        {
            //PopulateProductDetailsCommand = new AsyncRelayCommand(PopulateProductDetailsAsync);
            GetResultsCommand = new AsyncRelayCommand(GetResultsAsync);
            ViewProductCommand = new AsyncRelayCommand(ViewProduct);

            PackageList.CollectionChanged += Products_CollectionChanged;
        }

        private async void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!UpdateResultsList)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                    var newItems = e.NewItems.OfType<PackageViewModel>();
                    for (int i = 0; i < newItems.Count(); i++)
                    {
                        var pvm = newItems.ElementAt(i);
                        //var pd = await GetProductDetailsAsync(pvm.Package, Culture, Region);
                        //if (pd != null)
                        //{
                        //    ProductDetails[e.NewStartingIndex + i].Package = pd.Product;
                        //}
                    }

                    WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
                    break;
            }
        }

        public SearchResultsViewModel(string query) : this()
        {
            Query = query;
        }

        private readonly NavigationServiceBase NavService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly ISettingsService Settings = Ioc.Default.GetRequiredService<ISettingsService>();

        private string _Query;
        public string Query
        {
            get => _Query;
            set
            {
                SetProperty(ref _Query, value);
                GetResultsCommand.Execute(null);
            }
        }

        private bool _NoResults = false;
        public bool NoResults
        {
            get => _NoResults;
            set => SetProperty(ref _NoResults, value);
        }

        private ObservableCollection<PackageViewModel> _PackageList = new();
        public ObservableCollection<PackageViewModel> PackageList
        {
            get => _PackageList;
            set => SetProperty(ref _PackageList, value);
        }

        private PackageViewModel _SelectedProductDetails;
        public PackageViewModel SelectedPackage
        {
            get => _SelectedProductDetails;
            set => SetProperty(ref _SelectedProductDetails, value);
        }

        private IAsyncRelayCommand _PopulateProductDetailsCommand;
        public IAsyncRelayCommand PopulateProductDetailsCommand
        {
            get => _PopulateProductDetailsCommand;
            set => SetProperty(ref _PopulateProductDetailsCommand, value);
        }

        private IAsyncRelayCommand _GetSuggestionsCommand;
        public IAsyncRelayCommand GetResultsCommand
        {
            get => _GetSuggestionsCommand;
            set => SetProperty(ref _GetSuggestionsCommand, value);
        }

        private IAsyncRelayCommand _ViewProductCommand;
        public IAsyncRelayCommand ViewProductCommand
        {
            get => _ViewProductCommand;
            set => SetProperty(ref _ViewProductCommand, value);
        }

        public async Task GetResultsAsync()
        {
            try
            {
                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

                NoResults = false;
                PackageList.Clear();
                IEnumerable<PackageBase> results = await PackageService.SearchAsync(Query);

                if (Settings.UseExclusionFilter)
                {
                    // Filter out unwanted search results
                    Regex exclusionFilter = new(Settings.ExclusionFilter, RegexOptions.Compiled);
                    results = results.Where(pb => !exclusionFilter.IsMatch(pb.Title));
                }

                PackageList = new ObservableCollection<PackageViewModel>(results.Select(p => new PackageViewModel(p)));
                NoResults = PackageList.Count <= 0;

                WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                NavService.ShowHttpErrorPage(ex);
            }
        }

        public async Task ViewProduct()
        {
            GetResultsCommand.Cancel();
            UpdateResultsList = false;

            // No need to try-catch this, ViewPackage does this internally
            await SelectedPackage.ViewPackage();
        }
    }
}

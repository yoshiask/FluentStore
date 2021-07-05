using FluentStore.SDK;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        public SearchResultsViewModel(string query)
        {
            //PopulateProductDetailsCommand = new AsyncRelayCommand(PopulateProductDetailsAsync);
            GetResultsCommand = new AsyncRelayCommand(GetResultsAsync);
            ViewProductCommand = new AsyncRelayCommand(ViewProduct);
            Query = query;
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

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

        private ObservableCollection<PackageViewModel> _PackageList = new ObservableCollection<PackageViewModel>();
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
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            PackageList.Clear();
            var results = await PackageService.SearchAsync(Query);
            PackageList = new ObservableCollection<PackageViewModel>(results.Collapse().Select(p => new PackageViewModel(p)));

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task ViewProduct()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            GetResultsCommand.Cancel();
            UpdateResultsList = false;

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
            NavService.Navigate("PackageView", SelectedPackage);
        }
    }
}

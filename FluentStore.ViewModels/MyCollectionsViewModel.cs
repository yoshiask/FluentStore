using FluentStore.Services;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class MyCollectionsViewModel : ObservableRecipient
    {
        public MyCollectionsViewModel()
        {
            ViewCollectionCommand = new AsyncRelayCommand(ViewCollectionAsync);
            LoadCollectionsCommand = new AsyncRelayCommand(LoadCollectionsAsync);
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        private readonly FluentStoreAPI.FluentStoreAPI FSApi = Ioc.Default.GetRequiredService<FluentStoreAPI.FluentStoreAPI>();

        private ObservableCollection<CollectionViewModel> _Collections;
        public ObservableCollection<CollectionViewModel> Collections
        {
            get => _Collections;
            set => SetProperty(ref _Collections, value);
        }

        private CollectionViewModel _SelectedCollection;
        public CollectionViewModel SelectedCollection
        {
            get => _SelectedCollection;
            set => SetProperty(ref _SelectedCollection, value);
        }

        private IAsyncRelayCommand _ViewCollectionCommand;
        public IAsyncRelayCommand ViewCollectionCommand
        {
            get => _ViewCollectionCommand;
            set => SetProperty(ref _ViewCollectionCommand, value);
        }

        private IAsyncRelayCommand _LoadCollectionsCommand;
        public IAsyncRelayCommand LoadCollectionsCommand
        {
            get => _LoadCollectionsCommand;
            set => SetProperty(ref _LoadCollectionsCommand, value);
        }

        public async Task ViewCollectionAsync()
        {
            NavService.Navigate(SelectedCollection);
        }

        public async Task LoadCollectionsAsync()
        {
            var collections = await FSApi.GetCollectionsAsync(UserService.CurrentUser.LocalID);
            Collections = new ObservableCollection<CollectionViewModel>(
                collections.Select(c => new CollectionViewModel(c)));
        }
    }
}

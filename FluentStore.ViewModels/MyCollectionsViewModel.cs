using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
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

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("My Collections"));
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

        public async Task UpdateCollectionAsync(Collection newCollection)
        {
            // Make sure collection has a unique ID
            if (newCollection.Id == Guid.Empty)
                newCollection.Id = Guid.NewGuid();
            await FSApi.UpdateCollectionAsync(UserService.CurrentFirebaseUser.LocalID, newCollection);
            await LoadCollectionsAsync();
        }

        public async Task LoadCollectionsAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            var collections = await FSApi.GetCollectionsAsync(UserService.CurrentFirebaseUser.LocalID);
            Collections = new ObservableCollection<CollectionViewModel>(
                collections.Select(c => new CollectionViewModel(c)));

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }
    }
}

using FluentStore.SDK;
using FluentStore.SDK.Packages;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using FluentStoreAPI.Models;
using Garfoot.Utilities.FluentUrn;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentStore.SDK.Users;

namespace FluentStore.ViewModels
{
    public class MyCollectionsViewModel : ObservableRecipient
    {
        public MyCollectionsViewModel()
        {
            ViewCollectionCommand = new AsyncRelayCommand(ViewCollectionAsync);
            LoadCollectionsCommand = new AsyncRelayCommand(LoadCollectionsAsync);

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Collections"));
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        private ObservableCollection<PackageViewModel> _Collections = new();
        public ObservableCollection<PackageViewModel> Collections
        {
            get => _Collections;
            set => SetProperty(ref _Collections, value);
        }

        private PackageViewModel _SelectedCollection;
        public PackageViewModel SelectedCollection
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

        private bool _ShowNewCollectionTip = false;
        public bool ShowNewCollectionTip
        {
            get => _ShowNewCollectionTip;
            set => SetProperty(ref _ShowNewCollectionTip, value);
        }

        public async Task ViewCollectionAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            try
            {
                // FIXME: Implement a new method on PackageBase for fetching more detailed info
                // Get the author's display name
                //var package = SelectedCollection.Package;
                //var authorProfile = await FSApi.GetUserProfileAsync(package.Model.AuthorId);
                //package.Update(authorProfile);

                //// Load items
                //foreach (string urn in package.Model.Items)
                //{
                //    // Load the product details for each item
                //    var item = await PackageService.GetPackageAsync(Urn.Parse(urn));
                //    package.Items.Add(item);
                //}

                NavService.Navigate(SelectedCollection);
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new SDK.Messages.ErrorMessage(ex, SelectedCollection.Package));
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task UpdateCollectionAsync(Collection newCollection)
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));
            try
            {
                //await FSApi.UpdateCollectionAsync(UserService.CurrentUser.LocalID, newCollection);
                await LoadCollectionsAsync();
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                // TODO: Show error in InfoBar
                NavService.ShowHttpErrorPage(ex);
            }
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));
        }

        public async Task LoadCollectionsAsync()
        {
            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(true));

            Collections.Clear();
            try
            {
                var collections = await PackageService.GetCollectionsAsync();
                foreach (PackageBase collection in collections)
                {
                    Collections.Add(new PackageViewModel(collection));
                }
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                Collections.Clear();
                NavService.ShowHttpErrorPage(ex);
            }

            WeakReferenceMessenger.Default.Send(new PageLoadingMessage(false));

            ShowNewCollectionTip = Collections.Count <= 0;
        }
    }
}

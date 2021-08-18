using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Messaging;
using FluentStore.SDK;

namespace FluentStore.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        public ShellViewModel()
        {
            GetSearchSuggestionsCommand = new AsyncRelayCommand(GetSearchSuggestionsAsync);
            SubmitQueryCommand = new AsyncRelayCommand<PackageBase>(SubmitQueryAsync);
            SignInCommand = new AsyncRelayCommand(SignInAsync);
            SignOutCommand = new RelayCommand(UserService.SignOut);

            WeakReferenceMessenger.Default.Register<Messages.PageLoadingMessage>(this, (r, m) =>
            {
                // Handle the message here, with r being the recipient and m being the
                // input messenger. Using the recipient passed as input makes it so that
                // the lambda expression doesn't capture "this", improving performance.
                var self = (ShellViewModel)r;
                self.IsPageLoading = m.Value;
            });
            WeakReferenceMessenger.Default.Register<Messages.SetPageHeaderMessage>(this, (r, m) =>
            {
                var self = (ShellViewModel)r;
                self.PageHeader = m.Value;
            });
        }

        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();

        private string _PageHeader;
        public string PageHeader
        {
            get => _PageHeader;
            set => SetProperty(ref _PageHeader, value);
        }

        private bool _IsPageLoading;
        public bool IsPageLoading
        {
            get => _IsPageLoading;
            set => SetProperty(ref _IsPageLoading, value);
        }

        private ObservableCollection<PackageBase> _SearchSuggestions = new ObservableCollection<PackageBase>();
        public ObservableCollection<PackageBase> SearchSuggestions
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

        private PackageBase _SelectedPackage;
        public PackageBase SelectedPackage
        {
            get => _SelectedPackage;
            set => SetProperty(ref _SelectedPackage, value);
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

        private IAsyncRelayCommand<PackageBase> _SubmitQueryCommand;
        public IAsyncRelayCommand<PackageBase> SubmitQueryCommand
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

        private IRelayCommand _SignOutCommand;
        public IRelayCommand SignOutCommand
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
            var r = await PackageService.GetSearchSuggestionsAsync(SearchBoxText);
            if (r == null || r.Count <= 0)
            {
                SearchSuggestions = new ObservableCollection<PackageBase>
                {
                    new SDK.Packages.ModernPackage<object> { Title = "No results found" }
                };
            }
            else
            {
                SearchSuggestions = new ObservableCollection<PackageBase>(r);
            }
        }

        public async Task SubmitQueryAsync(PackageBase package)
        {
            SelectedPackage = package;
            NavService.Navigate("PackageView", SelectedPackage);
        }

        public async Task SignInAsync() => await UserService.TrySignIn();
    }
}

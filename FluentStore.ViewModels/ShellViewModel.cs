using FluentStore.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.SDK;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FluentStore.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        public ShellViewModel()
        {
            GetSearchSuggestionsCommand = new AsyncRelayCommand(GetSearchSuggestionsAsync);
            SubmitQueryCommand = new AsyncRelayCommand<PackageViewModel>(SubmitQueryAsync);
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
        }

        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly ISettingsService Settings = Ioc.Default.GetRequiredService<ISettingsService>();

        private bool _IsPageLoading;
        public bool IsPageLoading
        {
            get => _IsPageLoading;
            set => SetProperty(ref _IsPageLoading, value);
        }

        private ObservableCollection<PackageViewModel> _SearchSuggestions = new();
        public ObservableCollection<PackageViewModel> SearchSuggestions
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

        private PackageViewModel _SelectedPackage;
        public PackageViewModel SelectedPackage
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

        private IAsyncRelayCommand<PackageViewModel> _SubmitQueryCommand;
        public IAsyncRelayCommand<PackageViewModel> SubmitQueryCommand
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

        public async Task GetSearchSuggestionsAsync()
        {
            try
            {
                IEnumerable<PackageBase> results = await PackageService.GetSearchSuggestionsAsync(SearchBoxText);

                if (Settings.UseExclusionFilter)
                {
                    // Filter out unwanted search results
                    Regex exclusionFilter = new(Settings.ExclusionFilter, RegexOptions.Compiled);
                    results = results.Where(pb => !exclusionFilter.IsMatch(pb.Title));
                }

                if (results.Count() <= 0)
                {
                    SearchSuggestions = new ObservableCollection<PackageViewModel>
                    {
                        new PackageViewModel(new SDK.Packages.ModernPackage<object> { Title = "No results found" })
                    };
                }
                else
                {
                    SearchSuggestions = new ObservableCollection<PackageViewModel>(results.Select(pb => new PackageViewModel(pb)));
                }
            }
            catch (Flurl.Http.FlurlHttpException ex)
            {
                // TODO: Should this really navigate to a different page?
                NavService.ShowHttpErrorPage(ex);
            }
        }

        public async Task SubmitQueryAsync(PackageViewModel pvm)
        {
            SelectedPackage = pvm;

            // No need to try-catch this, ViewPackage does this internally
            await pvm.ViewPackage();
        }

        public async Task SignInAsync() => await UserService.TrySignIn();
    }
}

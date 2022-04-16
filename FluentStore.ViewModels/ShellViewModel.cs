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
using FluentStore.SDK.Messages;

namespace FluentStore.ViewModels
{
    public class ShellViewModel : ObservableRecipient
    {
        public ShellViewModel()
        {
            GetSearchSuggestionsCommand = new AsyncRelayCommand(GetSearchSuggestionsAsync);
            SubmitQueryCommand = new AsyncRelayCommand<PackageViewModel>(SubmitQueryAsync);

            WeakReferenceMessenger.Default.Register<Messages.PageLoadingMessage>(this, (r, m) =>
            {
                var self = (ShellViewModel)r;
                self.IsPageLoading = m.Value;
            });
        }

        private readonly PackageService PackageService = Ioc.Default.GetRequiredService<PackageService>();
        private readonly ISettingsService Settings = Ioc.Default.GetRequiredService<ISettingsService>();
        private readonly ObservableCollection<PackageViewModel> NoResultsCollection = new()
        {
            new(new SDK.Packages.GenericPackage<object>(null) { Title = "No results found" })
        };

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

        public async Task GetSearchSuggestionsAsync()
        {
            try
            {
                if (SearchBoxText.StartsWith("urn:") && SDK.Helpers.Extensions.TryParseUrn(SearchBoxText, out var urn))
                {
                    // User specified a URN, not a search term
                    PackageBase? package = await PackageService.TryGetPackageAsync(urn);
                    SearchSuggestions = package != null
                        ? new(new[] { new PackageViewModel(package) })
                        : NoResultsCollection;
                    return;
                }

                IEnumerable<PackageBase> results = await PackageService.GetSearchSuggestionsAsync(SearchBoxText);

                if (Settings.UseExclusionFilter)
                {
                    // Filter out unwanted search results
                    Regex exclusionFilter = new(Settings.ExclusionFilter, RegexOptions.Compiled);
                    results = results.Where(pb => !exclusionFilter.IsMatch(pb.Title));
                }

                SearchSuggestions = results.Any()
                    ? new(results.Select(pb => new PackageViewModel(pb)))
                    : NoResultsCollection;
            }
            catch (System.Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new ErrorMessage(ex, SearchBoxText, ErrorType.HandlerSearchSuggestFailed));
            }
        }

        public async Task SubmitQueryAsync(PackageViewModel pvm)
        {
            SelectedPackage = pvm;

            // No need to try-catch this, ViewPackage does this internally
            await pvm.ViewPackage();
        }
    }
}

using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchResultsView : ViewBase
    {
        public SearchResultsView()
        {
            InitializeComponent();
            ViewModel = new SearchResultsViewModel();
        }
        public SearchResultsViewModel ViewModel
        {
            get => (SearchResultsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(SearchResultsViewModel), typeof(SearchResultsView), new PropertyMetadata(null));

        public override void OnNavigatedTo(object parameter)
        {
            switch (parameter)
            {
                case string query:
                    ViewModel.Query = query;
                    break;
            }

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage($"Apps - \"{ViewModel.Query}\""));
        }
    }
}

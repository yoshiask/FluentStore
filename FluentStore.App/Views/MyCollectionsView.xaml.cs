using FluentStore.ViewModels;
using FluentStoreAPI.Models;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Helpers.RequiresSignIn]
    public sealed partial class MyCollectionsView : Page
    {
        public MyCollectionsViewModel ViewModel
        {
            get => (MyCollectionsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(MyCollectionsViewModel), typeof(MyCollectionsView), new PropertyMetadata(null));

        public MyCollectionsView()
        {
            InitializeComponent();
            ViewModel = new MyCollectionsViewModel();
        }

        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            var editDialog = new EditCollectionDetailsDialog(new Collection(), Content.XamlRoot);
            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // User wants to save
                await ViewModel.UpdateCollectionAsync(editDialog.Collection);
            }
        }
    }
}

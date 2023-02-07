using FluentStore.ViewModels;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.WinUI.AbstractUI.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Helpers.RequiresSignIn]
    public sealed partial class MyCollectionsView : ViewBase
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate "New collection" button menu
            NewCollectionMenu.Items.Clear();
            foreach (var handler in ViewModel.GetPackageHandlersForNewCollections())
            {
                var menuItem = new MenuFlyoutItem
                {
                    Text = handler.DisplayName,
                    DataContext = handler
                };
                menuItem.Click += NewCollectionMenuItem_Click;

                NewCollectionMenu.Items.Add(menuItem);
            }
        }

        private async void NewCollectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not SDK.PackageHandlerBase handler)
                return;

            var newCollection = await handler.CreateCollectionAsync();
            var editDialog = new AbstractFormDialog(handler.CreateEditForm(newCollection), Content.XamlRoot);
            if (await editDialog.ShowAsync() == ContentDialogResult.Primary)
            {
                // User wants to save
                await ViewModel.UpdateCollectionAsync(handler, newCollection);
            }
        }
    }
}

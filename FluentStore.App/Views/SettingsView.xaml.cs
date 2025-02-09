using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FluentStore.Services;
using FluentStore.ViewModels.Messages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : ViewBase
    {
        private readonly NavigationServiceBase NavigationService = Ioc.Default.GetRequiredService<NavigationServiceBase>();

        private readonly Dictionary<Type, object> _pages = new()
        {
            { typeof(Settings.General), null },
            { typeof(Settings.Plugins), null },
            { typeof(Settings.Info), null },
        };

        public SettingsView()
        {
            Loaded += SettingsView_Loaded;

            this.InitializeComponent();

            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Settings"));
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        public override void OnNavigatedFrom(object parameter) => Helpers.Settings.Default.SaveAsync();

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            NavigationService.AppNavigateBack();
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            int idx = sender.MenuItems.IndexOf(args.SelectedItem);
            if (idx < 0 || idx >= _pages.Count) return;

            var cache = _pages.ElementAt(idx);

            var page = cache.Value;
            if (page == null)
            {
                var type = cache.Key;
                _pages[type] = page = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            }

            SettingsPresenter.Content = page;
        }
    }
}

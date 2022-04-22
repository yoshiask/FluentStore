using FluentStore.Helpers;
using FluentStore.Views;
using Flurl;
using CommunityToolkit.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

namespace FluentStore.Services
{
    public class NavigationService : INavigationService
    {
        public NavigationService()
        {
            Pages = new()
            {
                new()
                {
                    PageType = typeof(HomeView),
                    Icon = new SymbolIcon(Symbol.Home),
                    Title = "Home",
                    Path = "home",
                    Tooltip = "Explore featured apps"
                },

                new()
                {
                    PageType = typeof(MyAppsView),
                    Icon = new SymbolIcon(Symbol.AllApps),
                    Title = "My Apps",
                    Path = "myapps",
                    Tooltip = "View your installed apps"
                },

                new()
                {
                    PageType = typeof(MyCollectionsView),
                    Icon = new SymbolIcon(Symbol.List),
                    Title = "Collections",
                    Path = "collections",
                    Tooltip = "Explore and create app collections",
                    RequiresSignIn = true
                },

                new()
                {
                    PageType = typeof(Views.Auth.AccountsView),
                    Icon = new SymbolIcon(Symbol.Account),
                    Title = "Accounts",
                    Path = "accounts",
                    Tooltip = "Manage your accounts",
                },
            };
        }

        public Frame CurrentFrame { get; set; }

        public override void Navigate(Type page)
        {
            CurrentFrame.Navigate(page);
        }

        public override void Navigate(Type page, object parameter)
        {
            CurrentFrame.Navigate(page, parameter);
        }

        public override void Navigate(string page)
        {
            Type type = ResolveType(page);
            Guard.IsNotNull(type, nameof(page));
            Navigate(type);
        }

        public override void Navigate(string page, object parameter)
        {
            Type type = ResolveType(page);
            Guard.IsNotNull(type, nameof(page));
            Navigate(type, parameter);
        }

        public override void Navigate(object parameter)
        {
            if (parameter == null)
                return;
            string paramName = parameter.GetType().Name;
            Type type = ResolveType(paramName);
            Guard.IsNotNull(type, nameof(type));
            Navigate(type, parameter);
        }

        public override void NavigateBack()
        {
            if (CurrentFrame.CanGoBack)
                CurrentFrame.GoBack();
        }

        public override void NavigateForward()
        {
            if (CurrentFrame.CanGoForward)
                CurrentFrame.GoForward();
        }


        public override void AppNavigate(Type page)
        {
            App.Current.Window.SetAppContent(page);
        }

        public override void AppNavigate(Type page, object parameter)
        {
            // TODO: Implement parameters
            AppNavigate(page);
        }

        public override void AppNavigate(string page)
        {
            Type type = ResolveType(page);
            Guard.IsNotNull(type, nameof(page));
            AppNavigate(type);
        }

        public override void AppNavigate(string page, object parameter)
        {
            Type type = ResolveType(page);
            Guard.IsNotNull(type, nameof(page));
            AppNavigate(type, parameter);
        }

        public override void AppNavigate(object parameter)
        {
            string paramName = parameter.GetType().Name;
            Type type = ResolveType(paramName);
            Guard.IsNotNull(type, nameof(type));
            AppNavigate(type, parameter);
        }

        public override void AppNavigateBack()
        {
            App.Current.Window.NavigateBack();
        }

        public override void AppNavigateForward()
        {

        }


        public override async Task<bool> OpenInBrowser(Url url)
        {
            // Wrap in a try-catch block in order to prevent the
            // app from crashing from invalid links.
            // (specifically from project badges)
            try
            {
                return await OpenInBrowser(url.ToUri());
            }
            catch
            {
                return false;
            }
        }

        public override async Task<bool> OpenInBrowser(Uri uri)
        {
            return await Windows.System.Launcher.LaunchUriAsync(uri);
        }


        public override Type ResolveType(string name)
        {
            name = name.ReplaceLastOccurrence("Model", "");
            return Type.GetType("FluentStore.Views." + name);
        }
    }

    public static class PageInfoEx
    {
        public static PageInfo CreateFromNavigationViewItem(NavigationViewItem navItem)
        {
            var page = new PageInfo();
            page.Title = (navItem.Content == null) ? string.Empty : navItem.Content.ToString();
            page.Icon = navItem.Icon ?? new SymbolIcon(Symbol.Document);

            var tooltip = ToolTipService.GetToolTip(navItem);
            page.Tooltip = (tooltip == null) ? string.Empty : tooltip.ToString();

            return page;
        }

        public static PageInfo GetFromNavigationViewItem(NavigationViewItem navItem)
        {
            if (navItem.Tag is PageInfo pageInfo)
                return pageInfo;
            else
                return null;
        }

        public static NavigationViewItem GetNavigationViewItem(this PageInfo page)
        {
            var item = new NavigationViewItem()
            {
                Tag = page,
                Content = page.Title,
                Icon = (IconElement)page.Icon,
            };
            if (!string.IsNullOrWhiteSpace(page.Tooltip))
                ToolTipService.SetToolTip(item, new ToolTip { Content = page.Tooltip });
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(item, page.Title);

            return item;
        }
    }
}

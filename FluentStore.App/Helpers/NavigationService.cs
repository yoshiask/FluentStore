using FluentStore.Helpers;
using FluentStore.Views;
using CommunityToolkit.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using FluentStore.Controls;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

namespace FluentStore.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private IntPtr m_hwnd;

        public NavigationService(ICommonPathManager pathManager) : base(pathManager)
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

        public NavigationFrame CurrentFrame { get; set; }

        public NavigationFrame AppFrame { get; set; }

        public override void Navigate(Type page, object parameter = null)
        {
            Guard.IsNotNull(page, nameof(page));
            IsNavigating = true;
            CurrentFrame.Navigate(page, parameter);
            IsNavigating = false;
        }

        public override void Navigate(object parameter)
        {
            if (parameter == null)
                return;
            string paramName = parameter.GetType().Name;
            Type type = ResolveType(paramName);
            Navigate(type, parameter);
        }

        public override void NavigateBack()
        {
            if (!CurrentFrame.CanGoBack)
                return;

            IsNavigating = true;
            CurrentFrame.NavigateBack();
            IsNavigating = false;
        }

        public override void NavigateForward()
        {
            if (!CurrentFrame.CanGoForward)
                return;

            IsNavigating = true;
            CurrentFrame.NavigateForward();
            IsNavigating = false;
        }

        public override void AppNavigate(Type page, object parameter = null)
        {
            Guard.IsNotNull(page, nameof(page));
            AppFrame.Navigate(page, parameter);
        }

        public override void AppNavigate(object parameter)
        {
            string paramName = parameter.GetType().Name;
            Type type = ResolveType(paramName);
            AppNavigate(type, parameter);
        }

        public override void AppNavigateBack()
        {
            if (AppFrame.CanGoBack)
                AppFrame.NavigateBack();
        }

        public override void AppNavigateForward()
        {
            if (AppFrame.CanGoForward)
                AppFrame.NavigateForward();
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

        public override IntPtr GetMainWindowHandle() => m_hwnd;

        public override void SetMainWindowHandle(IntPtr hwnd) => m_hwnd = hwnd;
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

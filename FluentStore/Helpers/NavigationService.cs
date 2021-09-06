using FluentStore.Helpers;
using FluentStore.Views;
using Flurl;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;

namespace FluentStore.Services
{
    public class NavigationService : INavigationService
    {
        public NavigationService()
        {
            Pages = new List<PageInfo>
            {
                new PageInfo()
                {
                    PageType = typeof(HomeView),
                    Icon = new SymbolIcon(Symbol.Home),
                    Title = "Home",
                    Path = "home"
                },

                new PageInfo()
                {
                    PageType = typeof(MyAppsView),
                    Icon = new SymbolIcon(Symbol.AllApps),
                    Title = "My Apps",
                    Path = "myapps"
                },

                new PageInfo()
                {
                    PageType = typeof(MyCollectionsView),
                    Icon = new SymbolIcon(Symbol.List),
                    Title = "My Collections",
                    Path = "mycollections",
                    RequiresSignIn = true
                },
            };
        }

        public Frame CurrentFrame { get; set; }
        public Frame AppFrame { get; set; }


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
            string vmName = paramName.ReplaceLastOccurrence("ViewModel", "") + "View";
            Type type = ResolveType(vmName);
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
            AppFrame.Navigate(page);
        }

        public override void AppNavigate(Type page, object parameter)
        {
            AppFrame.Navigate(page, parameter);
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
            string vmName = paramName.ReplaceLastOccurrence("Model", "");
            Type type = ResolveType(vmName);
            Guard.IsNotNull(type, nameof(type));
            AppNavigate(type, parameter);
        }

        public override void AppNavigateBack()
        {
            if (AppFrame.CanGoBack)
                AppFrame.GoBack();
        }

        public override void AppNavigateForward()
        {
            if (AppFrame.CanGoForward)
                AppFrame.GoForward();
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


        public override Type ResolveType(string viewName)
        {
            return Type.GetType("FluentStore.Views." + viewName);
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
                Visibility = page.RequiresSignIn ? Visibility.Collapsed : Visibility.Visible,
            };
            ToolTipService.SetToolTip(item, new ToolTip { Content = page.Tooltip });
            Windows.UI.Xaml.Automation.AutomationProperties.SetName(item, page.Title);

            return item;
        }
    }
}

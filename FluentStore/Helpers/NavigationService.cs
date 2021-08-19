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
        public Frame CurrentFrame { get; set; }
        public Frame AppFrame { get; set; }


        public void Navigate(Type page)
        {
            CurrentFrame.Navigate(page);
        }

        public void Navigate(Type page, object parameter)
        {
            CurrentFrame.Navigate(page, parameter);
        }

        public void Navigate(string page)
        {
            Type type = Type.GetType("FluentStore.Views." + page);
            Guard.IsNotNull(type, nameof(page));
            Navigate(type);
        }

        public void Navigate(string page, object parameter)
        {
            Type type = Type.GetType("FluentStore.Views." + page);
            Guard.IsNotNull(type, nameof(page));
            Navigate(type, parameter);
        }

        public void Navigate(object parameter)
        {
            if (parameter == null)
                return;
            string paramName = parameter.GetType().Name;
            string vmName = paramName.ReplaceLastOccurrence("ViewModel", "") + "View";
            Type type = Type.GetType("FluentStore.Views." + vmName);
            Guard.IsNotNull(type, nameof(type));
            Navigate(type, parameter);
        }

        public void NavigateBack()
        {
            if (CurrentFrame.CanGoBack)
                CurrentFrame.GoBack();
        }

        public void NavigateForward()
        {
            if (CurrentFrame.CanGoForward)
                CurrentFrame.GoForward();
        }


        public void AppNavigate(Type page)
        {
            AppFrame.Navigate(page);
        }

        public void AppNavigate(Type page, object parameter)
        {
            AppFrame.Navigate(page, parameter);
        }

        public void AppNavigate(string page)
        {
            Type type = Type.GetType("FluentStore.Views." + page);
            Guard.IsNotNull(type, nameof(page));
            AppNavigate(type);
        }

        public void AppNavigate(string page, object parameter)
        {
            Type type = Type.GetType("FluentStore.Views." + page);
            Guard.IsNotNull(type, nameof(page));
            AppNavigate(type, parameter);
        }

        public void AppNavigate(object parameter)
        {
            string paramName = parameter.GetType().Name;
            string vmName = paramName.ReplaceLastOccurrence("Model", "");
            Type type = Type.GetType("FluentStore.Views." + vmName);
            Guard.IsNotNull(type, nameof(type));
            AppNavigate(type, parameter);
        }

        public void AppNavigateBack()
        {
            if (AppFrame.CanGoBack)
                AppFrame.GoBack();
        }

        public void AppNavigateForward()
        {
            if (AppFrame.CanGoForward)
                AppFrame.GoForward();
        }


        public async Task<bool> OpenInBrowser(string url)
        {
            // Wrap in a try-catch block in order to prevent the
            // app from crashing from invalid links.
            // (specifically from project badges)
            try
            {
                return await OpenInBrowser(new Uri(url));
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> OpenInBrowser(Uri uri)
        {
            return await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        public Tuple<Type, object> ParseProtocol(Url ptcl)
        {
            Type destination = typeof(HomeView);
            var defaultResult = new Tuple<Type, object>(destination, null);

            if (ptcl == null || string.IsNullOrWhiteSpace(ptcl.Path))
                return defaultResult;

            try
            {
                string scheme = ptcl.Path.Split(":")[0];
                string path;
                switch (scheme)
                {
                    case "http":
                        path = ptcl.ToString().Remove(0, 23);
                        break;

                    case "https":
                        path = ptcl.ToString().Remove(0, 24);
                        break;

                    case "fluentstore":
                        path = ptcl.ToString().Remove(0, scheme.Length + 3);
                        break;

                    default:
                        // Unrecognized protocol
                        return defaultResult;
                }
                if (path.StartsWith("/"))
                    path = path.Remove(0, 1);
                var queryParams = ptcl.QueryParams;

                string rootPath = path.Split('/', StringSplitOptions.RemoveEmptyEntries)[0];
                switch (rootPath)
                {
                    //case "nointernet":
                    //    return new Tuple<Type, object>(typeof(Views.Subviews.NoInternetPage), queryParams);

                    default:
                        PageInfo pageInfo = Pages.Find(p => p.Path == rootPath);
                        destination = pageInfo != null ? pageInfo.PageType : typeof(HomeView);
                        return new Tuple<Type, object>(destination, queryParams);
                }
            }
            catch
            {
                return defaultResult;
            }
        }


        public List<PageInfo> Pages = new List<PageInfo>
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

    public class PageInfo
    {
        public PageInfo() { }

        public PageInfo(string title, string subhead, IconElement icon)
        {
            Title = title;
            Subhead = subhead;
            Icon = icon;
        }

        public PageInfo(NavigationViewItem navItem)
        {
            Title = (navItem.Content == null) ? "" : navItem.Content.ToString();
            Icon = navItem.Icon ?? new SymbolIcon(Symbol.Document);

            var tooltip = ToolTipService.GetToolTip(navItem);
            Tooltip = (tooltip == null) ? "" : tooltip.ToString();
        }

        public static PageInfo CreateFromNavigationViewItem(NavigationViewItem navItem)
        {
            if (navItem.Tag is PageInfo pageInfo)
                return pageInfo;
            else
                return null;
        }

        public string Title { get; set; }
        public string Subhead { get; set; }
        public IconElement Icon { get; set; }
        public Type PageType { get; set; }
        public string Path { get; set; }
        public string Tooltip { get; set; }
        public bool RequiresSignIn { get; set; } = false;

        // Derived properties
        public NavigationViewItem NavViewItem
        {
            get
            {
                var item = new NavigationViewItem()
                {
                    Tag = this,
                    Content = Title,
                    Icon = Icon,
                    Visibility = RequiresSignIn ? Visibility.Collapsed : Visibility.Visible,
                };
                ToolTipService.SetToolTip(item, new ToolTip() { Content = Tooltip });
                Windows.UI.Xaml.Automation.AutomationProperties.SetName(item, Title);

                return item;
            }
        }
        public string Protocol
        {
            get
            {
                return "fluentstore://" + Path;
            }
        }
        public Uri IconAsset
        {
            get
            {
                return new Uri("ms-appx:///Assets/Icons/" + Path + ".png");
            }
        }
    }
}

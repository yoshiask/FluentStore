using Flurl;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    public abstract class INavigationService
    {
        public List<PageInfo> Pages { get; protected set; }

        public abstract void Navigate(Type page);

        public abstract void Navigate(Type page, object parameter);

        public abstract void Navigate(string page);

        public abstract void Navigate(string page, object parameter);

        public abstract void Navigate(object parameter);

        public abstract void NavigateBack();

        public abstract void NavigateForward();

        public abstract void AppNavigate(Type page);

        public abstract void AppNavigate(Type page, object parameter);

        public abstract void AppNavigate(string page);

        public abstract void AppNavigate(string page, object parameter);

        public abstract void AppNavigate(object parameter);

        public abstract void AppNavigateBack();

        public abstract void AppNavigateForward();

        public abstract Task<bool> OpenInBrowser(string url);

        public abstract Task<bool> OpenInBrowser(Uri uri);

        public abstract Type ResolveType(string viewName);

        public Tuple<Type, object> ParseProtocol(Url ptcl)
        {
            Type destination = ResolveType("HomeView");
            object parameter = null;
            var defaultResult = new Tuple<Type, object>(destination, null);

            if (ptcl == null || string.IsNullOrWhiteSpace(ptcl.Path))
                return defaultResult;

            try
            {
                switch (ptcl.Host)
                {
                    case "package":
                        destination = ResolveType("PackageView");
                        parameter = Urn.Parse(ptcl.PathSegments[0]);
                        break;

                    case "web":
                        destination = ResolveType("PackageView");
                        parameter = (Url)ptcl.Path.Substring(1);
                        break;

                    default:
                        PageInfo pageInfo = Pages.Find(p => p.Path == ptcl.Host);
                        destination = pageInfo?.PageType ?? ResolveType("HomeView");
                        parameter = ptcl.QueryParams;
                        break;
                }
            }
            catch
            {
                return defaultResult;
            }

            return new Tuple<Type, object>(destination, parameter);
        }
    }

    public class PageInfo
    {
        public PageInfo() { }

        public PageInfo(string title, string subhead, object icon)
        {
            Title = title;
            Subhead = subhead;
            Icon = icon;
        }

        public string Title { get; set; }
        public string Subhead { get; set; }
        public object Icon { get; set; }
        public Type PageType { get; set; }
        public string Path { get; set; }
        public string Tooltip { get; set; }
        public bool RequiresSignIn { get; set; } = false;
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

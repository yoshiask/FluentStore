using Flurl;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    public abstract class NavigationServiceBase
    {
        private readonly ICommonPathManager _pathManager;

        public NavigationServiceBase(ICommonPathManager pathManager)
        {
            _pathManager = pathManager;
        }

        public List<PageInfo> Pages { get; protected set; }

        public bool IsNavigating { get; protected set; }

        public abstract void Navigate(Type page, object parameter);

        public void Navigate(string page, object parameter)
        {
            Type type = ResolveType(page);
            Navigate(type, parameter);
        }

        public abstract void Navigate(object parameter);

        public abstract void NavigateBack();

        public abstract void NavigateForward();

        public abstract void AppNavigate(Type page, object parameter);

        public void AppNavigate(string page, object parameter)
        {
            Type type = ResolveType(page);
            AppNavigate(type, parameter);
        }

        public abstract void AppNavigate(object parameter);

        public abstract void AppNavigateBack();

        public abstract void AppNavigateForward();

        public void ShowHttpErrorPage(int errorCode, string errorMessage = null)
        {
            Navigate("HttpErrorPage", (errorCode, errorMessage));
        }

        public void ShowHttpErrorPage(Flurl.Http.FlurlHttpException flurlEx, string errorMessage = null)
        {
            int errorCode = 418;
            if (flurlEx.StatusCode.HasValue)
                errorCode = flurlEx.StatusCode.Value;
            errorMessage ??= flurlEx.Message;
            ShowHttpErrorPage(errorCode, errorMessage);
        }

        public async Task<bool> OpenInBrowser(Url url)
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

        public abstract Task<bool> OpenInBrowser(Uri uri);

        public abstract Type ResolveType(string viewName);

        public ProtocolResult ParseProtocol(Url ptcl, bool isFirstInstance)
        {
            ProtocolResult defaultResult = new()
            {
                Page = ResolveType("HomeView"),
                Parameter = null,
            };

            if (ptcl == null || string.IsNullOrWhiteSpace(ptcl.Path))
                return defaultResult;

            ProtocolResult result = new();

            try
            {
                switch (string.IsNullOrEmpty(ptcl.Host) ? ptcl.PathSegments[0] : ptcl.Host)
                {
                    case "package":
                        result.Page = ResolveType("PackageView");
                        result.Parameter = Urn.Parse(ptcl.PathSegments[0]);
                        break;

                    case "web":
                        result.Page = ResolveType("PackageView");
                        result.Parameter = (Url)ptcl.Path.Substring(1);
                        break;

                    case "auth":
                        if (isFirstInstance)
                        {
                            result.Page = ResolveType("Auth.AccountsView");
                            result.Parameter = ptcl;
                        }
                        else
                        {
                            result.RedirectActivation = true;
                        }
                        break;

                    case "crash":
                        result.Page = ResolveType("HttpErrorPage");

                        // Load error message from file
                        var errorFilePath = System.IO.Path.Combine(_pathManager.GetDefaultLogDirectory().FullName, ptcl.PathSegments[1]);
                        result.Parameter = System.IO.File.ReadAllText(errorFilePath);
                        break;

                    default:
                        PageInfo pageInfo = Pages.Find(p => p.Path == ptcl.Host);
                        result.Page = pageInfo?.PageType ?? ResolveType("HomeView");
                        result.Parameter = ptcl.QueryParams;
                        break;
                }
            }
            catch
            {
                return defaultResult;
            }

            return result;
        }

        public abstract IntPtr GetMainWindowHandle();

        public abstract void SetMainWindowHandle(IntPtr hwnd);
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

    public struct ProtocolResult
    {
        public Type Page;

        public object Parameter;

        public bool RedirectActivation;

        public override readonly string ToString() => $"{Page.Name} '{Parameter}'";
    }
}

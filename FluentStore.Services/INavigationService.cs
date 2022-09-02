using Flurl;
using Garfoot.Utilities.FluentUrn;
using System;
using System.Collections.Generic;
using System.Text;
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

        public abstract Task<bool> OpenInBrowser(Url url);

        public abstract Task<bool> OpenInBrowser(Uri uri);

        public abstract Type ResolveType(string viewName);

        public ProtocolResult ParseProtocol(Url ptcl, bool isFirstInstance)
        {
            ProtocolResult result = new()
            {
                Page = ResolveType("Home"),
                Parameter = null,
            };
            ProtocolResult defaultResult = result;

            if (ptcl == null || string.IsNullOrWhiteSpace(ptcl.Path))
                return defaultResult;

            try
            {
                switch (string.IsNullOrEmpty(ptcl.Host) ? ptcl.Path : ptcl.Host)
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
                        int code = 418;
                        string message = null;
                        if (ptcl.QueryParams.TryGetFirst("code", out object codeParam))
                            int.TryParse(codeParam.ToString(), out code);
                        if (ptcl.QueryParams.TryGetFirst("msg", out object messageParam))
                            message = Encoding.UTF8.GetString(Convert.FromBase64String(messageParam.ToString()));
                        if (ptcl.QueryParams.TryGetFirst("trace", out object traceParam))
                            message += "\r\n" + Encoding.UTF8.GetString(Convert.FromBase64String(messageParam.ToString()));
                        result.Parameter = (code, message);
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

    public class ProtocolResult
    {
        public Type Page { get; set; }
        public object Parameter { get; set; }
        public bool RedirectActivation { get; set; } = false;

        public override string ToString() => $"{Page.Name} '{Parameter}'";
    }
}

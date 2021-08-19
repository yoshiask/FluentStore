using Flurl;
using System;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    public interface INavigationService
    {
        void Navigate(Type page);

        void Navigate(Type page, object parameter);

        void Navigate(string page);

        void Navigate(string page, object parameter);

        void Navigate(object parameter);

        void NavigateBack();

        void NavigateForward();

        void AppNavigate(Type page);

        void AppNavigate(Type page, object parameter);

        void AppNavigate(string page);

        void AppNavigate(string page, object parameter);

        void AppNavigate(object parameter);

        void AppNavigateBack();

        void AppNavigateForward();

        Task<bool> OpenInBrowser(string url);

        Task<bool> OpenInBrowser(Uri uri);

        Tuple<Type, object> ParseProtocol(Url ptcl);
    }
}

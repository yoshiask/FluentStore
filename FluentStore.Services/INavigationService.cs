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

        void AppNavigate(Type page);

        void AppNavigate(Type page, object parameter);

        void AppNavigate(string page);

        void AppNavigate(string page, object parameter);

        Task<bool> OpenInBrowser(string url);

        Task<bool> OpenInBrowser(Uri uri);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK;
using FluentStore.SDK.Users;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FluentStore.ViewModels.Auth
{
    public class AccountsViewModel : ObservableObject
    {
        public AccountsViewModel()
        {

        }

        private readonly PackageService _pkgSvc = Ioc.Default.GetRequiredService<PackageService>();

        private ObservableCollection<AccountHandlerBase> _signedInAccountHandlers = new();
        public ObservableCollection<AccountHandlerBase> SignedInAccountHandlers
        {
            get => _signedInAccountHandlers;
            set => SetProperty(ref _signedInAccountHandlers, value);
        }

        private ObservableCollection<AccountHandlerBase> _otherAccountHandlers = new();
        public ObservableCollection<AccountHandlerBase> OtherAccountHandlers
        {
            get => _otherAccountHandlers;
            set => SetProperty(ref _otherAccountHandlers, value);
        }

        public void LoadAccountHandlers()
        {
            foreach (var handler in _pkgSvc.GetAccountHandlers())
            {
                // Listen to property changed events to detect when a sign in occurs.
                handler.OnLoginStateChanged += Handler_LoginStateChanged;

                if (handler.IsLoggedIn)
                    SignedInAccountHandlers.Add(handler);
                else
                    OtherAccountHandlers.Add(handler);
            }
        }

        public void Unload()
        {
            foreach (var handler in SignedInAccountHandlers.Union(OtherAccountHandlers))
            {
                handler.OnLoginStateChanged -= Handler_LoginStateChanged;
            }
        }

        public Task HandleAuthActivation(Flurl.Url url) => _pkgSvc.RouteAuthActivation(url);

        private void Handler_LoginStateChanged(AccountHandlerBase handler, bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                SignedInAccountHandlers.Add(handler);
                OtherAccountHandlers.Remove(handler);
            }
            else
            {
                OtherAccountHandlers.Add(handler);
                SignedInAccountHandlers.Remove(handler);
            }
        }
    }
}

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

        private ObservableCollection<AccountHandlerViewModel> _signedInAccountHandlers = new();
        public ObservableCollection<AccountHandlerViewModel> SignedInAccountHandlers
        {
            get => _signedInAccountHandlers;
            set => SetProperty(ref _signedInAccountHandlers, value);
        }

        private ObservableCollection<AccountHandlerViewModel> _otherAccountHandlers = new();
        public ObservableCollection<AccountHandlerViewModel> OtherAccountHandlers
        {
            get => _otherAccountHandlers;
            set => SetProperty(ref _otherAccountHandlers, value);
        }

        public void LoadAccountHandlers()
        {
            foreach (var handler in _pkgSvc.GetAccountHandlers())
            {
                // Wrap handler in a view model
                AccountHandlerViewModel handlerViewModel = new(handler);

                // Listen to property changed events to detect when a sign in occurs.
                handler.OnLoginStateChanged += Handler_LoginStateChanged;

                if (handler.IsLoggedIn)
                    SignedInAccountHandlers.Add(handlerViewModel);
                else
                    OtherAccountHandlers.Add(handlerViewModel);
            }
        }

        public void Unload()
        {
            foreach (var handlerViewModel in SignedInAccountHandlers.Union(OtherAccountHandlers))
            {
                handlerViewModel.Handler.OnLoginStateChanged -= Handler_LoginStateChanged;
            }
        }

        public Task HandleAuthActivation(Flurl.Url url) => _pkgSvc.RouteAuthActivation(url);

        private void Handler_LoginStateChanged(AccountHandlerBase handler, bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                var handlerViewModel = OtherAccountHandlers.First(vm => vm.Handler == handler);
                SignedInAccountHandlers.Add(handlerViewModel);
                OtherAccountHandlers.Remove(handlerViewModel);
            }
            else
            {
                var vm = SignedInAccountHandlers.First(vm => vm.Handler == handler);
                OtherAccountHandlers.Add(vm);
                SignedInAccountHandlers.Remove(vm);
            }
        }
    }
}

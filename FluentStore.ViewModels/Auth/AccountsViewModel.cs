using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.Users;
using System.Collections.ObjectModel;
using System.Linq;

namespace FluentStore.ViewModels.Auth
{
    public class AccountsViewModel : ObservableObject
    {
        public AccountsViewModel()
        {

        }

        private readonly AccountService _accSvc = Ioc.Default.GetRequiredService<AccountService>();

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
            foreach (var handler in _accSvc.AccountHandlers)
            {
                // Listen to property changed events to detect when a sign in occurs.
                handler.PropertyChanged += Handler_PropertyChanged;

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
                handler.PropertyChanged -= Handler_PropertyChanged;
            }
        }

        private void Handler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AccountHandlerBase.IsLoggedIn) || sender is not AccountHandlerBase handler)
                return;

            if (handler.IsLoggedIn)
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.AbstractUI.ViewModels;
using FluentStore.SDK.Users;

namespace FluentStore.ViewModels.Auth
{
    public class AccountHandlerViewModel : ObservableObject
    {
        private AccountHandlerBase _handler;

        public AccountHandlerViewModel(AccountHandlerBase handler)
        {
            _handler = handler;
        }

        public AccountHandlerBase Handler
        {
            get => _handler;
            set => SetProperty(ref _handler, value);
        }

        /// <inheritdoc cref="AccountHandlerBase.SignOutAsync"/>
        public IAsyncRelayCommand SignOutCommand => new AsyncRelayCommand(Handler.SignOutAsync);

        /// <inheritdoc cref="AccountHandlerBase.CreateManageAccountForm"/>
        public AbstractFormViewModel CreateManageAccountForm() => new(Handler.CreateManageAccountForm());

        /// <inheritdoc cref="AccountHandlerBase.CreateSignInForm"/>
        public AbstractFormViewModel CreateSignInForm() => new(Handler.CreateSignInForm());
    }
}

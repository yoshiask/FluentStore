using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentStore.SDK.AbstractUI.ViewModels;
using FluentStore.SDK.Users;

namespace FluentStore.ViewModels.Auth
{
    public partial class AccountHandlerViewModel(AccountHandlerBase handler) : ObservableObject
    {
        public AccountHandlerBase Handler
        {
            get => handler;
            set => SetProperty(ref handler, value);
        }

        /// <inheritdoc cref="AccountHandlerBase.SignOutAsync"/>
        public IAsyncRelayCommand SignOutCommand => new AsyncRelayCommand(Handler.SignOutAsync);

        /// <inheritdoc cref="AccountHandlerBase.CreateManageAccountForm"/>
        public AbstractFormViewModel CreateManageAccountForm() => new(Handler.CreateManageAccountForm());

        /// <inheritdoc cref="AccountHandlerBase.CreateSignInForm"/>
        public AbstractFormViewModel CreateSignInForm() => new(Handler.CreateSignInForm());

        /// <inheritdoc cref="AccountHandlerBase.CreateSignUpForm"/>
        public AbstractFormViewModel CreateSignUpForm() => new(Handler.CreateSignUpForm());
    }
}

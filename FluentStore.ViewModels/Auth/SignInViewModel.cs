using FSAPI = FluentStoreAPI.FluentStoreAPI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.Services;
using System;
using Flurl.Http;
using FluentStoreAPI;
using FluentStoreAPI.Models.Firebase;

namespace FluentStore.ViewModels.Auth
{
    public class SignInViewModel : ObservableRecipient
    {
        public SignInViewModel()
        {
            SignInCommand = new AsyncRelayCommand(SignInAsync);
            SignUpCommand = new AsyncRelayCommand(SignUpAsync);
        }

        private readonly UserService UserService = Ioc.Default.GetRequiredService<UserService>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();

        private string _Email;
        public string Email
        {
            get => _Email;
            set => SetProperty(ref _Email, value);
        }

        private string _Password;
        public string Password
        {
            get => _Password;
            set => SetProperty(ref _Password, value);
        }

        private bool _IsSigningIn;
        public bool IsSigningIn
        {
            get => _IsSigningIn;
            set => SetProperty(ref _IsSigningIn, value);
        }

        private string _FailReason;
        public string FailReason
        {
            get => _FailReason;
            set => SetProperty(ref _FailReason, value);
        }

        private IAsyncRelayCommand _SignInCommand;
        public IAsyncRelayCommand SignInCommand
        {
            get => _SignInCommand;
            set => SetProperty(ref _SignInCommand, value);
        }

        private IAsyncRelayCommand _SignUpCommand;
        public IAsyncRelayCommand SignUpCommand
        {
            get => _SignUpCommand;
            set => SetProperty(ref _SignUpCommand, value);
        }

        public async Task SignInAsync()
        {
            try
            {
                IsSigningIn = true;
                FailReason = null;
                var resp = await FSApi.SignInAsync(Email, Password);
                if (await UserService.SignInAsync(resp.IDToken, resp.RefreshToken))
                    NavService.Navigate("HomeView");
            }
            catch (FlurlHttpException ex)
            {
                var errorResp = await ex.GetErrorResponse();
                FailReason = UserSignInResponse.CommonErrors.GetMessage(errorResp.Message);
            }
            finally
            {
                IsSigningIn = false;
            }
        }

        public async Task SignUpAsync()
        {
            try
            {
                IsSigningIn = true;
                FailReason = null;
                var resp = await FSApi.SignUpAndCreateProfileAsync(Email, Password, new FluentStoreAPI.Models.Profile { DisplayName = Email });
                if (await UserService.SignInAsync(resp.IDToken, resp.RefreshToken))
                    NavService.Navigate("HomeView");
            }
            catch (FlurlHttpException ex)
            {
                var errorResp = await ex.GetErrorResponse();
                FailReason = UserSignInResponse.CommonErrors.GetMessage(errorResp.Message);
            }
            finally
            {
                IsSigningIn = false;
            }
        }
    }
}

using FSAPI = FluentStoreAPI.FluentStoreAPI;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using FluentStore.Services;

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
            var resp = await FSApi.SignInAsync(Email, Password);
            if (await UserService.SignInAsync(resp.IDToken, resp.RefreshToken))
                NavService.Navigate("HomeView");
        }

        public async Task SignUpAsync()
        {
            var resp = await FSApi.SignUpAsync(Email, Password);
            if (await UserService.SignInAsync(resp.IDToken, resp.RefreshToken))
                NavService.Navigate("HomeView");
        }
    }
}

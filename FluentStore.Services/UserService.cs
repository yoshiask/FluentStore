using FSAPI = FluentStoreAPI.FluentStoreAPI;
using FluentStoreAPI.Models.Firebase;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;

namespace FluentStore.Services
{
    public class UserService : ObservableObject
    {
        private User _CurrentFirebaseUser;
        public User CurrentFirebaseUser
        {
            get => _CurrentFirebaseUser;
            internal set => SetProperty(ref _CurrentFirebaseUser, value);
        }

        private FluentStoreAPI.Models.User _CurrentUser;
        public FluentStoreAPI.Models.User CurrentUser
        {
            get => _CurrentUser;
            set => SetProperty(ref _CurrentUser, value);
        }

        private FluentStoreAPI.Models.Profile _CurrentProfile;
        public FluentStoreAPI.Models.Profile CurrentProfile
        {
            get => _CurrentProfile;
            set => SetProperty(ref _CurrentProfile, value);
        }

        private bool _IsLoggedIn;
        public bool IsLoggedIn
        {
            get => _IsLoggedIn;
            internal set
            {
                SetProperty(ref _IsLoggedIn, value);
                OnLoginStateChanged?.Invoke(value);
            }
        }

        public delegate void OnLoginStateChangedHandler(bool isLoggedIn);
        public static event OnLoginStateChangedHandler OnLoginStateChanged;


        private readonly FSAPI FSApi = Ioc.Default.GetRequiredService<FSAPI>();
        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();
        private readonly IPasswordVaultService PasswordVaultService = Ioc.Default.GetRequiredService<IPasswordVaultService>();

        public async Task<bool> SignInAsync(string token, string refreshToken)
        {
            try
            {
                if (token == null)
                {
                    // Use refresh token to get a new token
                    FSApi.RefreshToken = refreshToken;
                    var resp = await FSApi.UseRefreshToken();
                    if (resp != null)
                    {
                        token = resp.IDToken;
                        refreshToken = resp.RefreshToken;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(resp);
                    }
                }

                FSApi.Token = token;
                FSApi.RefreshToken = refreshToken;
                CurrentFirebaseUser = (await FSApi.GetCurrentUserDataAsync()).First();
                CurrentUser = await FSApi.GetUserAsync(CurrentFirebaseUser.LocalID);
                CurrentProfile = await FSApi.GetUserProfileAsync(CurrentFirebaseUser.LocalID);

                PasswordVaultService.Add(new CredentialBase(CurrentFirebaseUser.LocalID, refreshToken));

                IsLoggedIn = true;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);

                FSApi.Token = null;
                FSApi.RefreshToken = null;
                IsLoggedIn = false;
            }
            return IsLoggedIn;
        }

        public async Task TrySignIn(bool useUi = true)
        {
            try
            {
                var loginCredential = PasswordVaultService.FindAllByResource(CredentialBase.DEFAULT_RESOURCE)[0];

                if (loginCredential != null)
                {
                    bool success = await SignInAsync(null, loginCredential.Password);
                    if (!success)
                        goto failed;
                }
            }
            catch
            {
                goto failed;
            }

        failed:
            if (useUi)
            {
                // There is no credential stored in the locker.
                // Display UI to get user credentials.
                NavService.Navigate("Auth.SignInView");
            }
        }

        public void SignOut()
        {
            PasswordVaultService.Remove(new CredentialBase(CurrentFirebaseUser.LocalID, FSApi.RefreshToken));

            IsLoggedIn = false;
            FSApi.Token = null;
            FSApi.RefreshToken = null;
            CurrentFirebaseUser = null;
        }
    }
}

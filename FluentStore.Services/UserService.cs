using FSAPI = FluentStoreAPI.FluentStoreAPI;
using FluentStoreAPI.Models.Firebase;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;

namespace FluentStore.Services
{
    public class UserService : ObservableObject
    {
        private User _CurrentFirebaseUser;
        public User CurrentUser
        {
            get => _CurrentFirebaseUser;
            internal set => SetProperty(ref _CurrentFirebaseUser, value);
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

                CurrentUser = (await FSApi.GetCurrentUserDataAsync())[0];
                CurrentProfile = await FSApi.GetUserProfileAsync(CurrentUser.LocalID);

                PasswordVaultService.Add(new CredentialBase(CurrentUser.LocalID, refreshToken));

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
            if (IsLoggedIn) return;

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
            IsLoggedIn = false;
            if (useUi)
            {
                // There is no credential stored in the locker.
                // Display UI to get user credentials.
                NavService.Navigate("Auth.SignInView");
            }
        }

        public void SignOut()
        {
            PasswordVaultService.Remove(new CredentialBase(CurrentUser.LocalID, FSApi.RefreshToken));

            IsLoggedIn = false;
            FSApi.Token = null;
            FSApi.RefreshToken = null;
            CurrentUser = null;
        }
    }
}

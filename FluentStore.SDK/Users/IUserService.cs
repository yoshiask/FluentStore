using System.Threading.Tasks;

namespace FluentStore.SDK.Users
{
    public interface IUserService
    {
        public User CurrentUser { get; internal set; }

        public bool IsLoggedIn { get; internal set; }

        public delegate void OnLoginStateChangedHandler(bool isLoggedIn);

        public Task<bool> SignInAsync(string token, string refreshToken);

        public Task TrySignInAsync(bool useUi = true);

        public Task SignOutAsync();
    }
}

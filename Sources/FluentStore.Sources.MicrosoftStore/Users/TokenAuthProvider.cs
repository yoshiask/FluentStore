using CommunityToolkit.Diagnostics;
using Microsoft.Graph;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    /// <summary>
    /// An AuthProvider to handle tokens obtained manually, i.e. from an OpenID auth flow.
    /// </summary>
    internal class TokenAuthProvider : IAuthenticationProvider
    {
        private readonly string _token;

        /// <summary>
        /// Creates an AuthProvider to handle a manually obtained token.
        /// </summary>
        /// <param name="token">The token to use for authentication.</param>
        /// <exception cref="System.ArgumentException"> When a null token is passed.</exception>
        public TokenAuthProvider(string token)
        {
            Guard.IsNotNull(token, nameof(token));
            _token = token;
        }

        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, _token);

            return Task.CompletedTask;
        }
    }
}

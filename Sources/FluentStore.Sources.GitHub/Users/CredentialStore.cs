using Octokit;
using System.Threading.Tasks;

namespace FluentStore.Sources.GitHub.Users
{
    internal class CredentialStore : ICredentialStore
    {
        private static CredentialStore _current;
        public static CredentialStore Current
        {
            get
            {
                if (_current == null)
                    _current = new();
                return _current;
            }
        }

        public string Token { get; set; }

        public Task<Credentials> GetCredentials() => Task.FromResult(Token != null ? new Credentials(Token) : null);
    }
}

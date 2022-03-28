using Octokit;
using System.Threading.Tasks;

namespace FluentStore.Sources.GitHub.Users
{
    public class CredentialStore : ICredentialStore
    {
        public string Token { get; set; }

        public Task<Credentials> GetCredentials() => Task.FromResult(Token != null ? new Credentials(Token) : null);
    }
}

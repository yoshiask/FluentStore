using FluentStore.SDK.Users;
using Garfoot.Utilities.FluentUrn;

namespace FluentStore.Sources.GitHub.Users
{
    public class GitHubAccount : Account
    {
        public Octokit.User GitHubUser { get; set; }

        public GitHubAccount(Octokit.User user = null)
        {
            if (user != null)
                Update(user);
        }

        public void Update(Octokit.User user)
        {
            Urn = new(GitHubAccountHandler.NAMESPACE_GHUSER, new RawNamespaceSpecificString(user.Id.ToString()));
            DisplayName = user.Name;
            GitHubUser = user;
        }
    }
}

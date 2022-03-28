using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.AbstractUI.Models;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Microsoft.Graph;
using Microsoft.Marketplace.Storefront.Contracts;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccountHandler : OpenIDAccountHandler<MicrosoftAccount>
    {
        public const string APPIDURI_MSGRAPH = "https://graph.microsoft.com/";
        public const string BASEURL_MSGRAPH = APPIDURI_MSGRAPH + "v1.0/";

        private GraphServiceClient _graphClient;

        public override string Id => "msal";

        public override string DisplayName => "Microsoft Account";

        protected override string Authority => "https://login.microsoftonline.com/common/v2.0";
        protected override string ClientId => Secrets.MSA_CLIENTID;
        protected override string ClientSecret => Secrets.MSA_CLIENTSECRET;
        protected override string SignUpUrl => "https://signup.live.com";

        public MicrosoftAccountHandler(IPasswordVaultService passwordVaultService) : base(passwordVaultService)
        {
            Scopes = new[]
            {
                APPIDURI_MSGRAPH + "User.Read.All",
            };
        }

        protected override async Task<Account> UpdateCurrentUser()
        {
            _graphClient = new(new TokenAuthProvider(Token));
            var user = await _graphClient.Me.Request().GetAsync();

            return new MicrosoftAccount(user);
        }

        public override AbstractForm CreateManageAccountForm()
        {
            INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
            return AbstractUIHelper.CreateOpenInBrowserForm("ManageCollection", "Manage your account on the website.",
                "https://account.microsoft.com/profile", navService);
        }

        /// <summary>
        /// Sets the token of the logged-in user.
        /// </summary>
        /// <param name="requestOptions">
        /// The <see cref="RequestOptions"/> to set authentication on.
        /// </param>
        public void AuthenticateRequest(RequestOptions requestOptions)
        {
            requestOptions.Token = Token;
        }
    }
}

using CommunityToolkit.Mvvm.DependencyInjection;
using FluentStore.SDK.AbstractUI;
using FluentStore.SDK.Users;
using FluentStore.Services;
using Microsoft.Graph;
using Microsoft.Marketplace.Storefront.Contracts;
using OwlCore.AbstractUI.Models;
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

        public override AbstractUICollection CreateManageAccountForm()
        {
            return AbstractUIHelper.CreateSingleButtonUI("ManageCollection", "ManageButton", "Manage account", "\uE8A7",
                async (sender, e) =>
                {
                    INavigationService navService = Ioc.Default.GetRequiredService<INavigationService>();
                    await navService.OpenInBrowser("https://account.microsoft.com/profile");
                });
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

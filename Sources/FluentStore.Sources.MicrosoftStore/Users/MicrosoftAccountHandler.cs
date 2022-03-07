using FluentStore.SDK.Users;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccountHandler : OpenIDAccountHandler<MicrosoftAccount>
    {
        public const string APPIDURI_MSGRAPH = "https://graph.microsoft.com/";
        public const string BASEURL_MSGRAPH = APPIDURI_MSGRAPH + "v1.0/";
        public const string NAMESPACE_MSACCOUNT = "msal";

        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_MSACCOUNT,
        };

        public override string DisplayName => "Microsoft Account";

        protected override string Authority => "https://login.microsoftonline.com/common/v2.0";
        protected override string ClientId => Secrets.MSA_CLIENTID;
        protected override string ClientSecret => Secrets.MSA_CLIENTSECRET;
        protected override string SignUpUrl => "https://signup.live.com";

        private GraphServiceClient _graphClient;

        public MicrosoftAccountHandler()
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

        /// <summary>
        /// Gets the current token.
        /// </summary>
        /// <remarks>
        /// For internal use only. See <see cref="MicrosoftStoreHandler"/>.
        /// </remarks>
        internal string GetToken() => Token;
    }
}

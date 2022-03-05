using FluentStore.SDK.Users;
using Flurl;
using Garfoot.Utilities.FluentUrn;
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

        public MicrosoftAccountHandler()
        {
            Scopes = new[]
            {
                APPIDURI_MSGRAPH + "User.Read",
            };
        }

        protected override async Task<Account> UpdateCurrentUser()
        {
            return new MicrosoftAccount
            {
                Urn = new(NAMESPACE_MSACCOUNT, new RawNamespaceSpecificString("test")),
                DisplayName = "Test Account",
                Email = "bob@example.com",
            };
        }
    }
}

using FluentStore.SDK.Users;
using Flurl;
using Garfoot.Utilities.FluentUrn;
using Microsoft.Identity.Client;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        protected override string Authority => "https://login.microsoftonline.com/common/v2.0";
        protected override string ClientId => Secrets.MSA_CLIENTID;
        protected override string ClientSecret => Secrets.MSA_CLIENTSECRET;

        public MicrosoftAccountHandler()
        {
            Scopes = new[]
            {
                APPIDURI_MSGRAPH + "User.Read",
            };
        }

        protected override async Task PopulateCurrentUser()
        {
            CurrentUser = new MicrosoftAccount
            {
                Urn = Urn.Parse($"urn:{NAMESPACE_MSACCOUNT}:test"),
                DisplayName = "Test Account",
                Email = "bob@example.com",
                Id = "test"
            };
        }
    }
}

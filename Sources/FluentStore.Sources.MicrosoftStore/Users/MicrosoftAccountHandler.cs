using FluentStore.SDK.Users;
using Flurl;
using Microsoft.Identity.Client;
using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccountHandler : OpenIDAccountHandler<MicrosoftAccount>
    {
        public const string NAMESPACE_MSACCOUNT = "msal";
        public override HashSet<string> HandledNamespaces => new()
        {
            NAMESPACE_MSACCOUNT,
        };

        protected override string Authority => "https://login.microsoftonline.com/common/v2.0";
        protected override string ClientId => Secrets.MSA_CLIENTID;
        protected override string ClientSecret => Secrets.MSA_CLIENTSECRET;
        protected override string[] Scopes => new[]
        {
            "User.Read",
        };

        protected override async Task OnSignInSuccess()
        {
            
        }

        protected override async Task OnSignOut()
        {
            
        }
    }
}

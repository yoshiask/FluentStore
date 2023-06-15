using FluentStore.SDK.Users;
using Microsoft.Graph.Models;

namespace FluentStore.Sources.MicrosoftStore.Users
{
    public class MicrosoftAccount : Account
    {
        public MicrosoftAccount(User user = null)
        {
            if (user != null)
                Update(user);
        }

        public void Update(User user)
        {
            Id = user.Id;
            DisplayName = user.DisplayName;
            Email = user.Mail;
        }
    }
}

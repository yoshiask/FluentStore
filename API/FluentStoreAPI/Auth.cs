using System;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        public async Task SignUpAsync(string email, string password)
        {
            var session = await _supabase.Auth.SignUp(email, password)
                ?? throw new Exception("Failed to sign up");

            Token = session.AccessToken;
            RefreshToken = session.RefreshToken;
        }

        public async Task SignInAsync(string email, string password)
        {
            var session = await _supabase.Auth.SignInWithPassword(email, password)
                ?? throw new Exception("Failed to sign in");

            Token = session.AccessToken;
            RefreshToken = session.RefreshToken;
        }

        /// <summary>
        /// Exchanges the <see cref="RefreshToken"/> to get new tokens.
        /// </summary>
        /// <remarks>Note that this does *not* update <see cref="Token"/> or <see cref="RefreshToken"/></remarks>
        public async Task UseRefreshToken()
        {
            await _supabase.Auth.RefreshToken();
        }

        /// <summary>
        /// Sends a password reset email.
        /// </summary>
        /// <returns>The user's email.</returns>
        public async Task<string> RequestPasswordResetAsync(string email)
        {
            await _supabase.Auth.ResetPasswordForEmail(email);
            return email;
        }

        public async Task ChangeEmailAsync(string newEmail)
        {
            await _supabase.Auth.Update(new()
            {
                Email = newEmail
            });
        }

        public async Task UpdateDisplayNameAsync(string? displayName)
        {
            await _supabase.Auth.Update(new()
            {
                Data = new()
                {
                    ["display_name"] = displayName ?? _supabase.Auth.CurrentUser!.Email!,
                }
            });
        }
    }
}

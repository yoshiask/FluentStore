using Supabase.Gotrue;
using System;
using System.Threading.Tasks;

namespace FluentStoreAPI;

public partial class FluentStoreApiClient
{
    public async Task<Guid?> SignUpAsync(string email, string password)
    {
        var session = await _supabase.Auth.SignUp(email, password);

        var id = session?.User?.Id
            ?? throw new Exception("Failed to sign up");

        return new(id);
    }

    public async Task SignInAsync(string email, string password)
    {
        var session = await _supabase.Auth.SignInWithPassword(email, password);

        if (session?.User?.Id is null)
            throw new Exception("Failed to sign in");
    }

    public async Task<Uri> GetGoogleSignInUrlAsync(string? redirectUrl)
    {
        var authState = await _supabase.Auth.SignIn(Supabase.Gotrue.Constants.Provider.Google, new()
        {
            RedirectTo = redirectUrl
        });
        return authState.Uri;
    }

    public async Task CompleteSignInFromUrlAsync(Uri uri) => await _supabase.Auth.GetSessionFromUrl(uri);

    /// <summary>
    /// Exchanges the <see cref="RefreshToken"/> to get new tokens.
    /// </summary>
    /// <remarks>Note that this does *not* update <see cref="Token"/> or <see cref="RefreshToken"/></remarks>
    public async Task UseRefreshToken() => await _supabase.Auth.RefreshToken();

    /// <summary>
    /// Sends a password reset email.
    /// </summary>
    /// <returns>The user's email.</returns>
    public async Task RequestPasswordResetAsync(string email, string? redirectUrl = null)
    {
        ResetPasswordForEmailOptions resetOptions = new(email)
        {
            RedirectTo = redirectUrl
        };
        await _supabase.Auth.ResetPasswordForEmail(resetOptions);
    }

    public async Task CompletePasswordResetAsync(Uri authUri, string newPassword)
    {
        await CompleteSignInFromUrlAsync(authUri);
        await _supabase.Auth.Update(new()
        {
            Password = newPassword
        });
    }

    public async Task ChangeEmailAsync(string newEmail)
    {
        await _supabase.Auth.Update(new()
        {
            Email = newEmail
        });
    }
}

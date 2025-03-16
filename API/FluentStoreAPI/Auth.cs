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

    /// <summary>
    /// Exchanges the <see cref="RefreshToken"/> to get new tokens.
    /// </summary>
    /// <remarks>Note that this does *not* update <see cref="Token"/> or <see cref="RefreshToken"/></remarks>
    public async Task UseRefreshToken() => await _supabase.Auth.RefreshToken();

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

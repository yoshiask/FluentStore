namespace FluentStoreAPI.Models;

public class UserInformation
{
    public string? Email { get; set; }

    public required Profile Profile { get; set; }
}

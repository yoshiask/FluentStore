using System;

namespace FluentStoreAPI.Models;

public class Profile
{
    public Guid Id { get; set; }

    public string? Email { get; set; }

    public string DisplayName { get; set; }
}

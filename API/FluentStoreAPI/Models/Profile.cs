using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace FluentStoreAPI.Models;

[Table("UserProfiles")]
public class Profile : BaseModel
{
    [PrimaryKey("uid")]
    public Guid Uid { get; set; }

    [Column("firebase_id")]
    public string? FirebaseId { get; set; }

    [Column("display_name")]
    public string? DisplayName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }
}

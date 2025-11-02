using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FluentStoreAPI.Models;

[Table("UserCollections")]
public class Collection : BaseModel, IEquatable<Collection>
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("modified_at")]
    public DateTimeOffset ModifiedAt { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("is_public")]
    public bool IsPublic { get; set; }
    
    [Column("tile_glyph")]
    public string? TileGlyph { get; set; }
    
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [Column("items")]
    public List<string> Items { get; set; } = [];

    [JsonIgnore]
    public bool IsPrivate => !IsPublic;

    public bool Equals(Collection? other)
    {
        if (other is null)
            return false;

        // Check simple properties, ignoring timestamps
        var sameProps = Id == other.Id
            && Name == other.Name
            && Description == other.Description
            && IsPublic == other.IsPublic
            && TileGlyph == other.TileGlyph
            && ImageUrl == other.ImageUrl
            && AuthorId == other.AuthorId
            && Items?.Count == other.Items?.Count;

        if (!sameProps)
            return false;

        if (Items is null)
            return true;

        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var otherItem = other.Items![i];
            if (item != otherItem)
                return false;
        }

        return true;
    }

    public static bool operator ==(Collection obj1, Collection obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (obj1 is null)
            return false;
        
        if (obj2 is null)
            return false;
        
        return obj1.Equals(obj2);
    }

    public static bool operator !=(Collection obj1, Collection obj2) => !(obj1 == obj2);

    public override bool Equals(object? obj) => Equals(obj as Collection);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Id.GetHashCode();
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ Description.GetHashCode();
            hashCode = (hashCode * 397) ^ IsPublic.GetHashCode();
            hashCode = (hashCode * 397) ^ TileGlyph?.GetHashCode() ?? -1;
            hashCode = (hashCode * 397) ^ ImageUrl?.GetHashCode() ?? -1;
            hashCode = (hashCode * 397) ^ AuthorId.GetHashCode();

            foreach (var item in Items)
                hashCode = (hashCode * 397) ^ item.GetHashCode();

            return hashCode;
        }
    }
}

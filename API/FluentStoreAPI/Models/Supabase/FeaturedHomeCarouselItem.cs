using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace FluentStoreAPI.Models.Supabase;

[Table("FeaturedHomeCarousel")]
public class FeaturedHomeCarouselItem : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("package_urn")]
    public string PackageUrn { get; set; }

    [Column("min_version")]
    public Version? MinVersion { get; set; }

    [Column("max_version")]
    public Version? MaxVersion { get; set; }
}

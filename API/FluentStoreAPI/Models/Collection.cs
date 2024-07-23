using Google.Apis.Firestore.v1.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class Collection
    {
        public Collection() { }

        internal Collection(Document d)
        {
            AuthorId = d.Fields[nameof(AuthorId)].StringValue;
            Description = d.Fields[nameof(Description)].StringValue;
            ImageUrl = d.Fields[nameof(ImageUrl)].StringValue;
            IsPublic = d.Fields[nameof(IsPublic)].BooleanValue ?? false;
            Name = d.Fields[nameof(Name)].StringValue;
            TileGlyph = d.Fields[nameof(TileGlyph)].StringValue;
            Items = d.Fields[nameof(Items)].ArrayValue.Values
                .Select(v => v.StringValue).ToList();
        }

        public Guid Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TileGlyph { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorId { get; set; }
        public List<string> Items { get; set; } = [];

        [Ignore]
        public bool IsPrivate => !IsPublic;

        public static implicit operator Document(Collection c)
        {
            return new()
            {
                Fields =
                {
                    [nameof(AuthorId)] = new() { StringValue = c.AuthorId.ToString() },
                    [nameof(Description)] = new() { StringValue = c.Description },
                    [nameof(ImageUrl)] = new() { StringValue = c.ImageUrl },
                    [nameof(IsPublic)] = new() { BooleanValue = c.IsPublic },
                    [nameof(Name)] = new() { StringValue = c.Name },
                    [nameof(TileGlyph)] = new() { StringValue = c.TileGlyph },
                    [nameof(Items)] = new()
                    {
                        ArrayValue = new()
                        {
                            Values = c.Items.Select(x => new Value() { StringValue = x }).ToList()
                        }
                    },
                }
            };
        }
    }
}

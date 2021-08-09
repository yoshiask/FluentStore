using FluentStoreAPI.Models.Firebase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class Collection
    {
        public Guid Id { get; set; }
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TileGlyph { get; set; }
        public string ImageUrl { get; set; }
        public string AuthorId { get; set; }
        public List<string> Items { get; set; }

        /// <summary>
        /// Used by <see cref="Document"/> for deserialization
        /// </summary>
        public void SetItems(List<object> objItems)
        {
            Items = objItems.Cast<string>().ToList();
        }

        public bool IsPrivate => !IsPublic;
    }
}

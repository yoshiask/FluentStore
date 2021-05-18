using FluentStoreAPI.Models.Firebase;
using System.Collections.Generic;
using System.Linq;

namespace FluentStoreAPI.Models
{
    public class Collection
    {
        public bool IsPublic { get; set; }
        public string Name { get; set; }
        public List<string> Items { get; set; }

        /// <summary>
        /// Used by <see cref="Document"/> for deserialization
        /// </summary>
        public void SetItems(List<object> objItems)
        {
            Items = objItems.Cast<string>().ToList();
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class Product
    {
        public bool Curated { get; set; }

        public string Source { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

        public List<Meta> Metas { get; set; }

        [JsonIgnore]
        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri("https:" + Url);
                }
                catch
                {
                    return null;
                }
            }
        }
        [JsonIgnore]
        public Uri ImageUri
        {
            get
            {
                try
                {
                    return new Uri("https:" + ImageUrl);
                }
                catch
                {
                    return null;
                }
            }
        }

        public override string ToString() => Title;
    }
}

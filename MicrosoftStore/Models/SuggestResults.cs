using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class SuggestResults
    {
        public string Source { get; set; }
        
        public bool FromCache { get; set; }

        public string Type { get; set; }

        public List<Product> Suggests { get; set; }
    }
}

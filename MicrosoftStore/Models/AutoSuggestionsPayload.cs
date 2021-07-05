using MicrosoftStore.Responses;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class AutoSuggestionsPayload : Payload
    {
        public List<ProductDetails> AssetSuggestions { get; set; }
        public List<string> SearchSuggestions { get; set; }
        public List<object> MerchandizingSuggestions { get; set; }
    }
}

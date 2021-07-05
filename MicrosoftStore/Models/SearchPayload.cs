using MicrosoftStore.Responses;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class SearchPayload : Payload
    {
        public List<Card> SearchResults { get; set; }
        public List<SearchFilter> FilterOptions { get; set; }
        public SearchFilter DepartmentOptions { get; set; }
        public string NextUri { get; set; }
    }
}

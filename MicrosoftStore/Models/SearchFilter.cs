using MicrosoftStore.Responses;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class SearchFilter : Payload
    {
        public string Id { get; set; }
        public bool AlwaysVisible { get; set; }
        public string Title { get; set; }
        public List<string> DependentFilters { get; set; }
        public List<SearchFilterChoice> Choices { get; set; }
    }

    public class SearchFilterChoice : Payload
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? State { get; set; }
    }
}

using MicrosoftStore.Responses;
using System;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class RecommendationsPayload : Payload
    {
        public List<Card> Cards { get; set; }
        public string AlgoName { get; set; }
        public string AlgoValue { get; set; }
        public string Id { get; set; }
        public string CuratedBGColor { get; set; }
        public string CuratedDescription { get; set; }
        public string CuratedFGColor { get; set; }
        public Uri CuratedImageUrl { get; set; }
        public string CuratedTitle { get; set; }
        public string CollectionItemType { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
    }
}

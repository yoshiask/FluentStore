using MicrosoftStore.Models;
using System.Collections.Generic;

namespace MicrosoftStore.Responses
{
    public class SuggestResponse
    {
        public string Query { get; set; }

        public List<SuggestResults> ResultSets { get; set; }
    }
}

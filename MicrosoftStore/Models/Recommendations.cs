using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftStore.Models
{
    public class Recommendations
    {
        public string Path { get; set; }
        public DateTimeOffset ExpiryUtc { get; set; }
        public RecommendationsPayload Payload { get; set; }
    }
}

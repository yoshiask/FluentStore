using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicrosoftStore.Models
{
    public class Review
    {
        public double Rating { get; set; }
        public int HelpfulPositive { get; set; }
        public int HelpfulNegative { get; set; }
        public Guid ReviewId { get; set; }
        public DateTimeOffset SubmittedDateTimeUtc { get; set; }
        public bool IsProductTrial { get; set; }
        public bool IsTakenDown { get; set; }
        public bool ViolationsFound { get; set; }
        public bool IsPublished { get; set; }
        public bool IsRevised { get; set; }
        public bool UpdatedSinceResponse { get; set; }
        public bool IsAppInstalled { get; set; }
    }
}

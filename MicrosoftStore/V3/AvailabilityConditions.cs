using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class AvailabilityConditions
    {
        public DateTimeOffset EndDate { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public List<string> ResourceSetIds { get; set; }
        public AvailabilityClientConditions ClientConditions { get; set; }
    }
}

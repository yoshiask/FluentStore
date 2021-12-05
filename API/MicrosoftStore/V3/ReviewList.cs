using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ReviewList : ListBase
    {
        public List<Review> Reviews { get; set; }
    }
}

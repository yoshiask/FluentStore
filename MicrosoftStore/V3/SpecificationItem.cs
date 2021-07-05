namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class SpecificationItem
    {
        public string Level { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValidationHint { get; set; }
        public bool IsValidationPassed { get; set; }
    }
}

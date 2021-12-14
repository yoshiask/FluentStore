namespace Microsoft.Marketplace.Storefront.Contracts
{
    public class FulfillmentData
    {
        public string ProductId { get; set; }
        public string WuBundleId { get; set; }
        public string WuCategoryId { get; set; }
        public string PackageFamilyName { get; set; }
        public string SkuId { get; set; }
        public object Content { get; set; }
    }
}

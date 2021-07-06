namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class ListBase
    {
        public string ListType { get; set; }
        public string ListId { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public string ContinuationToken { get; set; }
    }
}

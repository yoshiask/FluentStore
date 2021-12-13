namespace Microsoft.Marketplace.Storefront.Contracts.V1
{
    public class ErrorResponse
    {
        public string ErrorContext { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorLongDescription { get; set; }
    }
}

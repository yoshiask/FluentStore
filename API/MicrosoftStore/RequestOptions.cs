using System.Globalization;

namespace Microsoft.Marketplace.Storefront.Contracts
{
    public class RequestOptions
    {
        public string DeviceFamily { get; set; } = Constants.DEFAULT_DEVICEFAMILY;

        public string DeviceArchitecture { get; set; } = Constants.DEFAULT_ARCHITECTURE;

        public CultureInfo Culture { get; set; } = null;

        public double Version { get; set; } = 9.0;

        public string Token { get; set; } = null;
    }
}

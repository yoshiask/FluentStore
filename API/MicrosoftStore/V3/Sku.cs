﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Marketplace.Storefront.Contracts.V3
{
    public class Sku
    {
        public List<string> Actions { get; set; }
        public string AvailabilityId { get; set; }
        public string SkuType { get; set; }
        public double Price { get; set; }
        public string DisplayPrice { get; set; }
        public string StrikethroughPrice { get; set; }
        public string PromoMessage { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string ResourceSetId { get; set; }
        public bool IsPaymentInstrumentRequired { get; set; }
        [JsonProperty("FulfillmentData")]
        public string FulfillmentDataStr { get; set; }
        public string MSAPurchaseType { get; set; }
        public List<PackageRequirements> PackageRequirements { get; set; }
        public int RemainingDaysInTrial { get; set; }
        public List<string> HardwareRequirements { get; set; }
        public List<string> HardwareWarnings { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public string SkuButtonTitle { get; set; }
        public List<Availability> Availabilities { get; set; }
        public bool IsPreorder { get; set; }
        public bool IsRental { get; set; }
        public DateTimeOffset FirstAvailableDate { get; set; }
        public bool IsRepurchasable { get; set; }
        public string SkuId { get; set; }
        public List<SkuDisplayRank> SkuDisplayRanks { get; set; }
        public string SkuTitle { get; set; }
        public string Description { get; set; }

        private FulfillmentData _FulfillmentData;
        [JsonIgnore]
        public FulfillmentData FulfillmentData
        {
            get
            {
                if (_FulfillmentData == null)
                    _FulfillmentData = JsonConvert.DeserializeObject<FulfillmentData>(FulfillmentDataStr);
                return _FulfillmentData;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace StoreLib.Models
{
    public enum DCatEndpoint //Xbox, Production and Int are accessible on the publicly. I assume Dev is Corpnet only.
    {
        Production,
        Int,
        Xbox,
        XboxInt,
        Dev,
        OneP,
        OnePInt
    }

    public static class Endpoints //Defining these here to allow for easy updating across the codebase should any change.
    {
        public static readonly Uri FE3Delivery = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        public static readonly Uri FE3DeliverySecured = new Uri("https://fe3.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");
        public static readonly Uri FE3CRDelivery = new Uri("https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        public static readonly Uri FE3CRDeliverySecured = new Uri("https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");
        //the following endpoints have been reported but I have not personally seen them yet
        public static readonly Uri FE6Delivery = new Uri("https://fe6.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        public static readonly Uri FE6DeliverySecured = new Uri("https://fe6.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");
        public static readonly Uri FE6CRDelivery = new Uri("https://fe6cr.delivery.mp.microsoft.com/ClientWebService/client.asmx");
        public static readonly Uri FE6CRDeliverySecured = new Uri("https://fe6cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured");
        //the following endpoints have been spotted in the real world
        public static readonly Uri DCATProd = new Uri("https://displaycatalog.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATInt = new Uri("https://displaycatalog-int.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATXbox = new Uri("https://xbox-displaycatalog.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATXboxInt = new Uri("https://xbox-displaycatalog-int.mp.microsoft.com/v7.0/products");
        public static readonly Uri DCATDev = new Uri("https://displaycatalog-dev.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATOneP = new Uri("https://displaycatalog1p.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATOnePInt = new Uri("https://displaycatalog1p-int.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DisplayCatalogSearch = new Uri("https://displaycatalog.mp.microsoft.com/v7.0/productFamilies/autosuggest?market=US&languages=en-US&query=");
        public static readonly Uri DisplayCatalogSearchInt = new Uri("https://displaycatalog-int.mp.microsoft.com/v7.0/productFamilies/autosuggest?market=US&languages=en-US&query=");
        //the following endpoints have been reported but I have not personally seen them yet
        //scat means staging catalog in the same way that dcat means display catalog
        public static readonly Uri SCATProd = new Uri("https://stagingcatalog.mp.microsoft.com/v7.0/products/");
        public static readonly Uri SCATInt = new Uri("https://stagingcatalog-int.mp.microsoft.com/v7.0/products/");
        public static readonly Uri SCATDev = new Uri("https://stagingcatalog-dev.mp.microsoft.com/v7.0/products/");
        public static readonly Uri SCATPublishingProd = new Uri("https://stagingcatalogpublishing.mp.microsoft.com/v7.0/products/");
        public static readonly Uri SCATPublishingInt = new Uri("https://stagingcatalogpublishing-int.mp.microsoft.com/v7.0/products/");
        public static readonly Uri SCATPublishingDev = new Uri("https://stagingcatalogpublishing-dev.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATOnePPPE = new Uri("https://displaycatalog1p-ppe.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATPPE = new Uri("https://displaycatalog-ppe.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATMD = new Uri("https://displaycatalog.md.mp.microsoft.com/v7.0/products/");
        public static readonly Uri DCATXboxPPE = new Uri("https://xbox-displaycatalog-PPE.mp.microsoft.com/v7.0/products/");
        //pcats means product catalog service
        public static readonly Uri PCATSDevDPS = new Uri("https://productcatalogservice-dev.dps.mp.microsoft.com/v7.0/products/");
        public static readonly Uri PCATSDFDCE = new Uri("https://productcatalogservice-df.dce.mp.microsoft.com/v7.0/products/");
        public static readonly Uri PCATSDCE = new Uri("https://productcatalogservice.dce.mp.microsoft.com/v7.0/products/");    }
}

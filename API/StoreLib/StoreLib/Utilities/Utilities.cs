using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using StoreLib.Models;

namespace StoreLib.Utilities
{
    public static class TypeHelpers
    {
        /// <summary>
        /// Convert the provided DcatEndpoint enum to its matching Uri.
        /// </summary>
        /// <param name="EnumEndpoint"></param>
        /// <returns></returns>
        public static Uri EnumToUri(DCatEndpoint EnumEndpoint)
        {
            switch (EnumEndpoint)
            {
                case DCatEndpoint.Production:
                    return Endpoints.DCATProd;
                case DCatEndpoint.Dev:
                    return Endpoints.DCATDev;
                case DCatEndpoint.Int:
                    return Endpoints.DCATInt;
                case DCatEndpoint.Xbox:
                    return Endpoints.DCATXbox;
                case DCatEndpoint.XboxInt:
                    return Endpoints.DCATXboxInt;
                case DCatEndpoint.OneP:
                    return Endpoints.DCATOneP;
                case DCatEndpoint.OnePInt:
                    return Endpoints.DCATOnePInt;
                default:
                    return Endpoints.DCATProd;
            }
        }

        public static Uri EnumToSearchUri(DCatEndpoint Endpoint)
        {
            switch (Endpoint)
            {
                case DCatEndpoint.Production:
                    return Endpoints.DisplayCatalogSearch;
                case DCatEndpoint.Int:
                    return Endpoints.DisplayCatalogSearchInt;
                default:
                    return Endpoints.DisplayCatalogSearch;
            }
        }

        public static PackageType StringToPackageType(string raw)
        {
            switch (raw)
            {
                case "XAP":
                    return PackageType.XAP;
                case "AppX":
                    return PackageType.AppX;
                case "UAP":
                    return PackageType.UAP;
                default:
                    return PackageType.Unknown;
            }
        }
    }

    internal static class HashHelpers //These are used for the image caching function. The input uri will be hashed and used as the downloaded image file name. 
    {

        internal static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        internal static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }


    }

    public static class UriHelpers
    {
        /// <summary>
        /// Create and format the DCat request uri based on the provided endpoint, id, id type and locale. 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="ID"></param>
        /// <param name="IDType"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        public static Uri CreateAlternateDCatUri(DCatEndpoint endpoint, string ID, IdentiferType IDType, Services.Locale locale)
        {
            switch (IDType)
            {
                case IdentiferType.ContentID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=CONTENTID&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.LegacyWindowsPhoneProductID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=LegacyWindowsPhoneProductID&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.LegacyWindowsStoreProductID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=LegacyWindowsStoreProductID&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.LegacyXboxProductID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=LegacyXboxProductID&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.PackageFamilyName:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=PackageFamilyName&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.XboxTitleID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}lookup?alternateId=XboxTitleID&Value={ID}&{locale.DCatTrail}&fieldsTemplate=Details");
                case IdentiferType.ProductID:
                    return new Uri($"{TypeHelpers.EnumToUri(endpoint)}{ID}?{locale.DCatTrail}");
                default:
                    throw new Exception("CreateAlternateDCatUri: Unknown IdentifierType was passed, an update is likely required, please report this issue.");

            }


        }



    }
}

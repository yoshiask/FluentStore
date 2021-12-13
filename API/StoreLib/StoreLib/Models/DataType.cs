using System;
using System.Collections.Generic;
using System.Text;

namespace StoreLib.Models
{
    public enum PackageType
    {
        UAP,
        XAP,
        AppX,
        Unknown
    }

    public enum IdentiferType
    {
        ProductID,
        XboxTitleID,
        PackageFamilyName,
        ContentID,
        LegacyWindowsPhoneProductID,
        LegacyWindowsStoreProductID,
        LegacyXboxProductID
    }

    public enum ImagePurpose
    {
        Logo,
        Tile,
        Screenshot,
        BoxArt,
        BrandedKeyArt,
        Poster,
        FeaturePromotionalSquareArt,
        ImageGallery,
        SuperHeroArt,
        TitledHeroArt
    }

    public enum ProductKind
    {
        Game,
        Application,
        Book,
        Movie,
        Physical,
        Software
    }

    public enum DeviceFamily
    {
        Desktop,
        Mobile,
        Xbox,
        ServerCore,
        IotCore,
        HoloLens,
        Andromeda,
        Universal,
        WCOS
    }

    public enum DisplayCatalogResult
    {
        NotFound,
        Restricted,
        TimedOut, 
        Error,
        Found
    }
}

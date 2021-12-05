
using System;
using System.Collections.Generic;
using System.Text;

namespace StoreLib.Models
{
    public class Addon
    {
        public string ProductID;
        public ProductKind ProductType;
        public string DisplayName;

        public Addon(string ProductID, ProductKind ProductType, string DisplayName)
        {
            this.ProductID = ProductID;
            this.ProductType = ProductType;
            this.DisplayName = DisplayName;
        }
    }
}

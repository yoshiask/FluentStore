using Microsoft.Marketplace.Storefront.Contracts.V2;
using CommunityToolkit.Diagnostics;
using FluentStore.SDK.Images;

namespace FluentStore.Sources.MicrosoftStore
{
    public class MicrosoftStoreImage : FileImage
    {
        public MicrosoftStoreImage(ImageItem img = null)
        {
            if (img != null)
                Update(img);
        }

        public void Update(ImageItem img)
        {
            Guard.IsNotNull(img, nameof(img));

            BackgroundColor = img.BackgroundColor;
            ForegroundColor = img.ForegroundColor;
            Url = img.Url;
            ImageType = (ImageType)img.ImageType;
            Height = img.Height;
            Width = img.Width;
            ImagePositionInfo = img.ImagePositionInfo;
            Caption = img.Caption;
        }

        private string _ImagePositionInfo = "";
        public string ImagePositionInfo
        {
            get => _ImagePositionInfo;
            set => SetProperty(ref _ImagePositionInfo, value);
        }
    }
}

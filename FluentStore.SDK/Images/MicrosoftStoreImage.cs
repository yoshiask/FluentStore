using Microsoft.Marketplace.Storefront.Contracts.V2;

namespace FluentStore.SDK.Images
{
    public class MicrosoftStoreImage : FileImage
    {
        public MicrosoftStoreImage(ImageItem img)
        {
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

        private string _Caption;
        public string Caption
        {
            get => _Caption;
            set => SetProperty(ref _Caption, value);
        }
    }
}

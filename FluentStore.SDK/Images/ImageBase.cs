using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentStore.SDK.Images
{
    public class ImageBase : ObservableObject
    {
        private ImageType _ImageType;
        public ImageType ImageType
        {
            get => _ImageType;
            set => SetProperty(ref _ImageType, value);
        }

        private int _Height;
        public int Height
        {
            get => _Height;
            set => SetProperty(ref _Height, value);
        }

        private int _Width;
        public int Width
        {
            get => _Width;
            set => SetProperty(ref _Width, value);
        }

        private string _BackgroundColor;
        public string BackgroundColor
        {
            get => _BackgroundColor;
            set => SetProperty(ref _BackgroundColor, value);
        }

        private string _ForegroundColor;
        public string ForegroundColor
        {
            get => _ForegroundColor;
            set => SetProperty(ref _ForegroundColor, value);
        }

        public override string ToString() => string.Empty;
    }

    public enum ImageType
    {
        Unspecified,
        BoxArt,
        Logo,
        Poster,
        Tile,
        Hero,
        Screenshot,
        Trailer
    }
}

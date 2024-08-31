using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentStore.SDK.Images
{
    public partial class ImageBase : ObservableObject
    {
        [ObservableProperty]
        private ImageType _imageType;

        [ObservableProperty]
        private int _height;

        [ObservableProperty]
        private int _width;

        [ObservableProperty]
        private string _backgroundColor;

        [ObservableProperty]
        private string _foregroundColor;

        [ObservableProperty]
        private string _caption;

        public override string ToString() => Caption;
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

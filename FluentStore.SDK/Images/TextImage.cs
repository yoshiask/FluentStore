using FluentStore.SDK.Models;

namespace FluentStore.SDK.Images
{
    public class TextImage : ImageBase
    {
        private string _Text;
        public string Text
        {
            get => _Text;
            set => SetProperty(ref _Text, value);
        }
    }
}

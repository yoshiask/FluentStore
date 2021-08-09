using System;
using System.IO;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentStore.Converters
{
    public class StreamToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Stream stream))
                throw new ArgumentException($"{nameof(value)} must be of type {nameof(Stream)}");

            var bitmap = new BitmapImage();
            stream.Seek(0, SeekOrigin.Begin);
            bitmap.SetSourceAsync(stream.AsRandomAccessStream()).GetResults();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

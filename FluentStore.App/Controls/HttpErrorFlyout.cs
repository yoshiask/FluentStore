using FluentStore.Views;
using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public class HttpErrorFlyout : Flyout
    {
        public HttpErrorFlyout(int errorCode, string errorMessage = null)
        {
            Content = new HttpErrorPage(errorCode, errorMessage)
            {
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };
        }

        public int ErrorCode
        {
            get => ((HttpErrorPage)Content).ErrorCode;
            set => ((HttpErrorPage)Content).ErrorCode = value;
        }

        public string ErrorMessage
        {
            get => ((HttpErrorPage)Content).ErrorMessage;
            set => ((HttpErrorPage)Content).ErrorMessage = value;
        }
    }
}

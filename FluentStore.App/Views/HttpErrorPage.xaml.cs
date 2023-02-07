using Microsoft.UI.Xaml;

namespace FluentStore.Views
{
    /// <summary>
    /// An page that displays an HTTP error code, along with a brief message.
    /// </summary>
    public sealed partial class HttpErrorPage : ViewBase
    {
        public HttpErrorPage()
        {
            this.InitializeComponent();
        }

        public HttpErrorPage(int errorCode, string errorMessage = null) : this()
        {
            ErrorCode = errorCode;
            UpdateErrorTitle(this);
            ErrorMessage = errorMessage;
        }

        public int ErrorCode
        {
            get => (int)GetValue(ErrorCodeProperty);
            set => SetValue(ErrorCodeProperty, value);
        }
        public static readonly DependencyProperty ErrorCodeProperty = DependencyProperty.Register(
            nameof(ErrorCode), typeof(int), typeof(HttpErrorPage), new PropertyMetadata(418, OnErrorCodeChanged));

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            nameof(ErrorMessage), typeof(string), typeof(HttpErrorPage), new PropertyMetadata("I'm a teapot"));

        public string ErrorTitle
        {
            get => (string)GetValue(ErrorTitleProperty);
            set => SetValue(ErrorTitleProperty, value);
        }
        public static readonly DependencyProperty ErrorTitleProperty = DependencyProperty.Register(
            nameof(ErrorTitle), typeof(string), typeof(HttpErrorPage), new PropertyMetadata(string.Empty));

        private static void UpdateErrorTitle(HttpErrorPage page, int? errorCode = null)
        {
            // Set error message to HTTP status code names as listed by IANA
            // https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
            if (SDK.Models.WebException.HttpStatusCodes.TryGetValue(errorCode ?? page.ErrorCode, out string ianaErrorMessage))
                page.ErrorTitle = ianaErrorMessage;
        }

        public override void OnNavigatedTo(object parameter)
        {
            if (parameter is (int errorCode, string errorMessage))
            {
                ErrorCode = errorCode;
                ErrorMessage = errorMessage;
                UpdateErrorTitle(this, errorCode);
            }
        }

        private static void OnErrorCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateErrorTitle((HttpErrorPage)d, e.NewValue as int?);
        }
    }
}

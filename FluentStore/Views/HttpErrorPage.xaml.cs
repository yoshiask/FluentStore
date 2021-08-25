using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentStore.Views
{
    /// <summary>
    /// An page that displays an HTTP error code, along with a brief message.
    /// </summary>
    public sealed partial class HttpErrorPage : Page
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

        protected static void UpdateErrorTitle(HttpErrorPage page, int? errorCode = null)
        {
            // Set error message to HTTP status code names as listed by IANA
            // https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml
            if (HttpStatusCodes.TryGetValue(errorCode ?? page.ErrorCode, out string ianaErrorMessage))
                page.ErrorTitle = ianaErrorMessage;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is (int errorCode, string errorMessage))
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

        public static readonly Dictionary<int, string> HttpStatusCodes = new Dictionary<int, string>
        {
            { 100, "Continue" },
            { 101, "Switching Protocols" },
            { 102, "Processing" },
            { 103, "Early Hints" },
            { 200, "OK" },
            { 201, "Created" },
            { 202, "Accepted" },
            { 203, "Non-Authoritative Information" },
            { 204, "No Content" },
            { 205, "Reset Content" },
            { 206, "Partial Content" },
            { 207, "Multi-Status" },
            { 208, "Already Reported" },
            { 226, "IM Used" },
            { 300, "Multiple Choices" },
            { 301, "Moved Permanently" },
            { 302, "Found" },
            { 303, "See Other" },
            { 304, "Not Modified" },
            { 305, "Use Proxy" },
            { 306, "Switch Proxy" },
            { 307, "Temporary Redirect" },
            { 308, "Permanent Redirect" },
            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 402, "Payment Required" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 405, "Method Not Allowed" },
            { 406, "Not Acceptable" },
            { 407, "Proxy Authentication Required" },
            { 408, "Request Timeout" },
            { 409, "Conflict" },
            { 410, "Gone" },
            { 411, "Length Required" },
            { 412, "Precondition Failed" },
            { 413, "Payload Too Large" },
            { 414, "URI Too Long" },
            { 415, "Unsupported Media Type" },
            { 416, "Range Not Satisfiable" },
            { 417, "Expectation Failed" },
            { 418, "Unknown" },     // "I'm a teapot"
            { 421, "Misdirected Request" },
            { 422, "Unprocessable Entity" },
            { 423, "Locked" },
            { 424, "Failed Dependency" },
            { 425, "Too Early" },
            { 426, "Upgrade Required" },
            { 427, "Unassigned" },
            { 428, "Precondition Required" },
            { 429, "Too Many Requests" },
            { 430, "Unassigned" },
            { 431, "Request Header Fields Too Large" },
            { 451, "Unavailable For Legal Reasons" },
            { 500, "Internal Server Error" },
            { 501, "Not Implemented" },
            { 502, "Bad Gateway" },
            { 503, "Service Unavailable" },
            { 504, "Gateway Timeout" },
            { 505, "HTTP Version Not Supported" },
            { 506, "Variant Also Negotiates" },
            { 507, "Insufficient Storage" },
            { 508, "Loop Detected" },
            { 509, "Unassigned" },
            { 510, "Not Extended" },
            { 511, "Network Authentication Required" },
        };
    }
}

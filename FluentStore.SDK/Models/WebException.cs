using System;

namespace FluentStore.SDK.Models
{
    public class WebException : Exception
    {
        public int StatusCode { get; set; }

        public WebException(int code, string message = null) : base(message)
        {
            StatusCode = code;
        }
    }
}

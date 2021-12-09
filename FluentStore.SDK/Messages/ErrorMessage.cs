using CommunityToolkit.Mvvm.Messaging.Messages;
using System;

namespace FluentStore.SDK.Messages
{
    public class ErrorMessage<T> : ValueChangedMessage<Exception> where T : class
    {
        public ErrorMessage(Exception ex, T context = null) : base(ex)
        {
            Context = context;
        }

        public Exception Exception => Value;
        public T Context { get; set; }
        public ErrorType Type { get; set; }
    }

    public enum ErrorType
    {
        None,
        PackageFetchFailed,
        PackageDownloadFailed,
        PackageInstallFailed,
    }

    public class ErrorMessage : ErrorMessage<object>
    {
        public ErrorMessage(Exception ex, object context = null) : base(ex, context)
        {
        }
    }
}

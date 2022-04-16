using System;

namespace FluentStore.SDK.Messages
{
    public class ErrorMessage
    {
        public ErrorMessage(Exception ex, object context = null, ErrorType type = ErrorType.None)
        {
            Exception = ex;
            Context = context;
            Type = type;
        }

        public Exception Exception { get; set; }
        public object Context { get; set; }
        public ErrorType Type { get; set; }
    }

    public enum ErrorType
    {
        None,
        PackageFetchFailed,
        PackageDownloadFailed,
        PackageInstallFailed,
        PackageLaunchFailed,
        PackageSaveFailed,
        PackageDeleteFailed,
        HandlerSearchSuggestFailed,
        HandlerSearchFailed,
    }
}

// https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/2a2336d21773be4bfeaa49141e5490718e25fa6e/Uwp/UwpSample/SystemBrowser.cs

using System;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;
using System.Threading;
using FluentStore.Services;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace FluentStore.SDK.Users.Browsers
{
    public class SystemBrowser : IBrowser
    {
        static TaskCompletionSource<BrowserResult> inFlightRequest;
        public Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            inFlightRequest?.TrySetCanceled(cancellationToken);
            inFlightRequest = new TaskCompletionSource<BrowserResult>();

            NavigationServiceBase navService = Ioc.Default.GetRequiredService<NavigationServiceBase>();
            navService.OpenInBrowser(options.StartUrl);

            return inFlightRequest.Task;
        }

        public static void ProcessResponse(Uri responseData)
        {
            var result = new BrowserResult
            {
                Response = responseData.OriginalString,
                ResultType = BrowserResultType.Success
            };
            
            inFlightRequest.SetResult(result);
            inFlightRequest = null; 
        }
    }
}

// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

// https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/2a2336d21773be4bfeaa49141e5490718e25fa6e/Uwp/UwpSample/WabBrowser.cs

using CommunityToolkit.Diagnostics;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace FluentStore.SDK.Users.Browsers
{
    public class WabBrowser : IBrowser
    {
        private readonly bool _enableWindowsAuthentication;

        public WabBrowser(bool enableWindowsAuthentication = false)
        {
            _enableWindowsAuthentication = enableWindowsAuthentication;
        }

        private async Task<BrowserResult> InvokeAsyncCore(BrowserOptions options, bool silentMode)
        {
            var wabOptions = WebAuthenticationOptions.None;

            if (options.ResponseMode == OidcClientOptions.AuthorizeResponseMode.FormPost)
            {
                wabOptions |= WebAuthenticationOptions.UseHttpPost;
            }
            if (_enableWindowsAuthentication)
            {
                wabOptions |= WebAuthenticationOptions.UseCorporateNetwork;
            }
            if (silentMode)
            {
                wabOptions |= WebAuthenticationOptions.SilentMode;
            }

            WebAuthenticationResult wabResult;

            try
            {
                if (string.Equals(options.EndUrl, WebAuthenticationBroker.GetCurrentApplicationCallbackUri().AbsoluteUri, StringComparison.Ordinal))
                {
                    wabResult = await WebAuthenticationBroker.AuthenticateAsync(
                        wabOptions, new Uri(options.StartUrl));
                }
                else
                {
                    wabResult = await WebAuthenticationBroker.AuthenticateAsync(
                        wabOptions, new Uri(options.StartUrl), new Uri(options.EndUrl));
                }
            }
            catch (Exception ex)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.ToString()
                };
            }

            if (wabResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.Success,
                    Response = wabResult.ResponseData
                };
            }
            else if (wabResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.HttpError,
                    Error = string.Concat(wabResult.ResponseErrorDetail.ToString())
                };
            }
            else if (wabResult.ResponseStatus == WebAuthenticationStatus.UserCancel)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UserCancel
                };
            }
            else
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = "Invalid response from WebAuthenticationBroker"
                };
            }
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNullOrWhiteSpace(options.StartUrl, nameof(options.StartUrl));
            Guard.IsNotNullOrWhiteSpace(options.EndUrl, nameof(options.EndUrl));

            switch (options.DisplayMode)
            {
                case DisplayMode.Visible:
                    return await InvokeAsyncCore(options, false);

                case DisplayMode.Hidden:
                    var result = await InvokeAsyncCore(options, true);
                    if (result.ResultType == BrowserResultType.Success)
                    {
                        return result;
                    }
                    else
                    {
                        result.ResultType = BrowserResultType.Timeout;
                        return result;
                    }

                default:
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(options.DisplayMode));
                    return null;
            }
        }
    }
}
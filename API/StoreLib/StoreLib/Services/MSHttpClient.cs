using StoreLib.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace StoreLib.Services
{
    public class MSHttpClient : HttpClient
    {
        private readonly CorrelationVector _cv = new CorrelationVector();

        private static readonly bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                                                    .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        private static HttpClientHandler _handler
        {
            get
            {
                HttpClientHandler handler = new HttpClientHandler();
                if (!IsWindows)
                {
                    handler.ServerCertificateCustomValidationCallback = ServerCertificateValidationCallback;
                }
                return handler;
            }
        }

        private static bool ServerCertificateValidationCallback(
            object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors
        )
        {

            // TODO: Refine
            return true;
        }

        /// <summary>
        /// Instantiate MSHttpClient
        /// </summary>
        public MSHttpClient()
            : base(_handler)
        {
            _cv.Init();
            base.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "StoreLib");
        }

        /// <summary>
        /// An override of the SendAsync Function from HttpClient. This is done to automatically add the needed MS-CV tracking header to every request (along with our user-agent).
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) //Overriding the SendAsync so we can easily add the CorrelationVector and User-Agent to every request. 
        {
            request.Headers.Add("MS-CV", _cv.GetValue());
            _cv.Increment();
            HttpResponseMessage response = await base.SendAsync(request);
            return response;
        }
    }
}

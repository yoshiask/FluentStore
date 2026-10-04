using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.Services;

internal class RewritingHttpHandler(HttpMessageHandler next, IEnumerable<HttpRequestUriRewriteRule> rewriteEntries) : DelegatingHandler(next)
{
    public IReadOnlyCollection<HttpRequestUriRewriteRule> RewriteEntries { get; } = [..rewriteEntries];

#if NET5_0_OR_GREATER
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var originalUri = request.RequestUri;

        foreach (var uri in EnumerateUriRewrites(request.RequestUri.ToString()))
        {
            request.RequestUri = uri;
            var response = base.Send(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;
        }

        request.RequestUri = originalUri;
        return base.Send(request, cancellationToken);
    }
#endif

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var originalUri = request.RequestUri;

        foreach (var uri in EnumerateUriRewrites(request.RequestUri.ToString()))
        {
            request.RequestUri = uri;
            var response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;
        }

        request.RequestUri = originalUri;
        return await base.SendAsync(request, cancellationToken);
    }

    private IEnumerable<Uri> EnumerateUriRewrites(string originalUri)
    {
        foreach (var entry in RewriteEntries)
        {
            if (!originalUri.StartsWith(entry.OriginalPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            yield return new Uri(entry.NewPrefix + originalUri.Substring(entry.OriginalPrefix.Length));
        }
    }
}

internal class HttpRequestUriRewriteRule(string originalPrefix, string newPrefix)
{
    public string OriginalPrefix { get; } = originalPrefix;
    public string NewPrefix { get; } = newPrefix;
}

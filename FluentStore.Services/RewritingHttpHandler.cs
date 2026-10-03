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
        request.RequestUri = new Uri(RewriteUri(request.RequestUri.ToString()));
        return base.Send(request, cancellationToken);
    }
#endif

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.RequestUri = new Uri(RewriteUri(request.RequestUri.ToString()));
        return await base.SendAsync(request, cancellationToken);
    }

    private string RewriteUri(string originalUri)
    {
        foreach (var entry in RewriteEntries)
        {
            if (!originalUri.StartsWith(entry.OriginalPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            return entry.NewPrefix + originalUri.Substring(entry.OriginalPrefix.Length);
        }

        return originalUri;
    }
}

internal class HttpRequestUriRewriteRule(string originalPrefix, string newPrefix)
{
    public string OriginalPrefix { get; } = originalPrefix;
    public string NewPrefix { get; } = newPrefix;
}

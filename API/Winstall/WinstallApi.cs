using AngleSharp;
using Flurl;
using Flurl.Http;
using FuseSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Winstall.Models;

namespace Winstall;

public class WinstallApi
{
    private AppPageResponse _app;
    private Url _currentApiBase;
    private readonly Fuse _fuse = new();

    public async Task UpdateIfOutdated(CancellationToken cancellationToken = default)
    {
        if (await IsCacheOutdated(cancellationToken))
            await UpdateCacheAsync(cancellationToken);
    }

    public async Task UpdateCacheAsync(CancellationToken cancellationToken = default)
    {
        // Winstall uses unique build IDs whenever it updates.
        // The current build ID is specified in a blob in the main page.
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);
        using var document = await context.OpenAsync(Constants.WINSTALL_HOST, cancellationToken);
        var nextDataScript = document.Scripts.First(s => s.Id == "__NEXT_DATA__");

        _app = Newtonsoft.Json.JsonConvert.DeserializeObject<AppPageResponse>(nextDataScript.InnerHtml);
        _currentApiBase = Constants.GetWinstallApiHost(_app.BuildId);
    }

    public async Task<IndexPageProps> GetIndexAsync(bool forceGet = false, CancellationToken cancellationToken = default)
    {
        await UpdateIfOutdated(cancellationToken);

        if (forceGet)
        {
            var response = await _currentApiBase.GetJsonAsync<Response<IndexPageProps>>(cancellationToken);
            return response.PageProps;
        }
        else
        {
            return _app.Props.PageProps;
        }
    }

    public async Task<PacksPageProps> GetPacksAsync(CancellationToken cancellationToken = default)
    {
        await UpdateIfOutdated();

        var response = await _currentApiBase
            .AppendPathSegments("packs.json")
            .GetJsonAsync<Response<PacksPageProps>>(cancellationToken);
        return response.PageProps;
    }

    public async Task<PackPageProps> GetPackAsync(string id, CancellationToken cancellationToken = default)
    {
        await UpdateIfOutdated();

        var response = await _currentApiBase
            .AppendPathSegments("packs", $"{id}.json")
            .GetJsonAsync<Response<PackPageProps>>(cancellationToken);
        return response.PageProps;
    }

    public async Task<AppPageProps> GetAppAsync(string id, CancellationToken cancellationToken = default)
    {
        await UpdateIfOutdated();

        var response = await _currentApiBase
            .AppendPathSegments("apps", $"{id}.json")
            .GetJsonAsync<Response<AppPageProps>>(cancellationToken);
        return response.PageProps;
    }

    public async Task<IEnumerable<App>> SearchAppsAsync(string query)
    {
        await UpdateIfOutdated();

        var allApps = _app.Props.PageProps.Apps;

        // TODO: FuseSharp throws an NRE when searching IFuseable
        var sorted = _fuse.Search(query, allApps.Select(a => a.Name));
        return sorted.Where(r => r.Score <= 0.4).Select(r => allApps[r.Index]);
    }

    private async Task<bool> IsCacheOutdated(CancellationToken cancellationToken = default)
    {
        if (_currentApiBase == null || _app == null)
            return true;

        try
        {
            Url testUrl = _currentApiBase.AppendPathSegments("index.json");
            var response = await testUrl.HeadAsync(cancellationToken);
            return response.ResponseMessage.IsSuccessStatusCode;
        }
        catch (FlurlHttpException)
        {
            return true;
        }
    }
}

using Newtonsoft.Json;

namespace Winstall.Models;

internal class Response<TPageProps> where TPageProps : class, IPageProps
{
    public TPageProps PageProps { get; set; }

    /// <summary>
    /// <seealso href="https://github.com/vercel/next.js/discussions/12558"/>
    /// </summary>
    [JsonProperty("__N_SSG")]
    public bool NSSG { get; set; }
}

using Flurl;
using WinGetRun.Models;

namespace WinGetRun
{
    public static class UrlEx
    {
        public static Url SetSearchOptions(this Url url, SearchOptions options)
        {
            SearchOptions defaultOpt = new SearchOptions();
            if (options == null || options == defaultOpt)
                return url;

            if (options.SplitQuery != defaultOpt.SplitQuery)
                url = url.SetQueryParam("splitQuery", options.SplitQuery);
            if (options.PartialMatch != defaultOpt.PartialMatch)
                url = url.SetQueryParam("partialMatch", options.PartialMatch);
            if (options.EnsureContains != defaultOpt.EnsureContains)
                url = url.SetQueryParam("ensureContains", options.EnsureContains);
            if (options.PreferContains != defaultOpt.PreferContains)
                url = url.SetQueryParam("preferContains", options.PreferContains);
            if (options.Sample != defaultOpt.Sample)
                url = url.SetQueryParam("sample", options.Sample);

            return url;
        }
        
        public static Url SetPaginationOptions(this Url url, PaginationOptions options)
        {
            PaginationOptions defaultOpt = new PaginationOptions();
            if (options == null || options == defaultOpt)
                return url;

            if (options.Take != defaultOpt.Take)
                url = url.SetQueryParam("take", options.Take);
            if (options.Page != defaultOpt.Page)
                url = url.SetQueryParam("page", options.Page);
            if (options.Sort != defaultOpt.Sort)
                url = url.SetQueryParam("sort", options.Sort);
            if (options.Order != defaultOpt.Order)
                url = url.SetQueryParam("order", options.Order);

            return url;
        }
    }
}

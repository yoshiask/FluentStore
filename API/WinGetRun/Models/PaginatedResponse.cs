using System.Collections.Generic;

namespace WinGetRun.Models
{
    public class PaginatedResponse
    {
        public List<PackageSearchResult> Packages { get; set; }
        public int Total { get; set; }
    }
}

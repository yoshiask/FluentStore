namespace WinGetRun.Models
{
    public class SearchOptions
    {
        /// <summary>
        /// This option splits the search query by whitespace and
        /// searched for each part separately.
        /// </summary>
        public bool SplitQuery { get; set; } = true;

        /// <summary>
        /// This option enabled the 'partial' part of fuzzy search;
        /// The query string will be split into ngrams which will be used
        /// for the search. This process is carried our on each part of
        /// the query if 'split query' is enabled.
        /// </summary>
        public bool PartialMatch { get; set; } = false;

        /// <summary>
        /// This option ensures that each final search result exactly contains
        /// the specified query, discarding non-matching results. Additionally,
        /// this option cannot be set if a non 'query' search paramter
        /// such as name, publisher, etc. is specified.
        /// </summary>
        public bool EnsureContains { get; set; } = false;

        /// <summary>
        /// This option functions similarly to 'ensure contains', however,
        /// instead of removing packages which don't exactly match the search query,
        /// results are re-shuffled based on how well they match the query.
        /// </summary>
        public bool PreferContains { get; set; } = false;

        /// <summary>
        /// The number of packages to sample when <see cref="PreferContains"/> is specified.
        /// This option exists in case packages outside of the number specified by
        /// 'take' could be a higher match. While the sorting logic will use 'sample'
        /// packages, only 'take' packages will be returned in the final response.
        /// </summary>
        public int Sample { get; set; } = 12;
    }
}

namespace WinGetRun.Models
{
    public class PaginationOptions
    {
        /// <summary>
        /// Represents the number of documents per page.
        /// </summary>
        public int Take { get; set; } = 12;

        /// <summary>
        /// Represents the page number (starting from 0) to return documents for.
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// Specifies the field to sort by.
        /// </summary>
        public string Sort { get; set; } = null;

        /// <summary>
        /// <c>1</c> is ascending, and <c>-1</c> is descending.
        /// </summary>
        public int Order { get; set; } = 1;
    }
}

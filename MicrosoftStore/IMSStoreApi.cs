using Refit;
using System.Threading.Tasks;
using MicrosoftStore.Responses;

namespace MicrosoftStore
{
    public interface IMSStoreApi
    {
        /// <summary>
        /// Performs a search using the given query
        /// </summary>
        [Get("/suggest")]
        Task<SuggestResponse> GetSuggestions(string query, string market, string clientId,
            [Query(CollectionFormat.Csv)] string[] sources,
            [Query(CollectionFormat.Csv)] int[] counts);
    }
}

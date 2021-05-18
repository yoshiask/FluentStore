using Microsoft.Toolkit.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class SearchResultsSource : IIncrementalSource<ProductDetailsViewModel>
    {
        private readonly List<ProductDetailsViewModel> _people;

        public SearchResultsSource()
        {
            // Creates an example collection.
            _people = new List<ProductDetailsViewModel>();

            for (int i = 1; i <= 200; i++)
            {
                var p = new ProductDetailsViewModel();// { Name = "ProductDetailsViewModel " + i };
                _people.Add(p);
            }
        }

        public async Task<IEnumerable<ProductDetailsViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            // Gets items from the collection according to pageIndex and pageSize parameters.
            var result = (from p in _people
                          select p).Skip(pageIndex * pageSize).Take(pageSize);

            return result;
        }
    }

}

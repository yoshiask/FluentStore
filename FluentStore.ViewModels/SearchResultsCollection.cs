using CommunityToolkit.Common.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentStore.ViewModels
{
    public class SearchResultsSource : IIncrementalSource<PackageViewModel>
    {
        private readonly List<PackageViewModel> _people;

        public SearchResultsSource()
        {
            // Creates an example collection.
            _people = new List<PackageViewModel>();

            for (int i = 1; i <= 200; i++)
            {
                var p = new PackageViewModel();// { Name = "ProductDetailsViewModel " + i };
                _people.Add(p);
            }
        }

        public async Task<IEnumerable<PackageViewModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            // Gets items from the collection according to pageIndex and pageSize parameters.
            var result = (from p in _people
                          select p).Skip(pageIndex * pageSize).Take(pageSize);

            return result;
        }
    }

}

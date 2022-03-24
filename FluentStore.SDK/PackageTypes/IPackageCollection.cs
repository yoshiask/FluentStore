using System.Collections.ObjectModel;

namespace FluentStore.SDK.Packages
{
    public interface IPackageCollection
    {
        public ObservableCollection<PackageBase> Items { get; set; }
    }
}

using System.Threading.Tasks;

namespace FluentStore
{
    public interface IAppContent
    {
        public bool IsCompact { get; }

        public void OnNavigatedTo() { }

        public void OnNavigatedFrom() { }
    }
}

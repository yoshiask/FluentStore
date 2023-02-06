namespace FluentStore
{
    public interface IAppContent
    {
        public bool IsCompact { get; }

        public void OnNavigatedTo(object parameter) { }

        public void OnNavigatedFrom(object parameter) { }
    }
}

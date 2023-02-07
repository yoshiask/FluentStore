using Microsoft.UI.Xaml.Controls;

namespace FluentStore.Views
{
    public abstract class ViewBase : UserControl, IAppContent
    {
        public bool IsCompact { get; protected set; }

        public virtual void OnNavigatedTo(object parameter) { }

        public virtual void OnNavigatedFrom(object parameter) { }
    }
}

using FluentStore.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.UI.Xaml;

namespace FluentStore.Themes
{
    public partial class PackageDisplayTemplates : ResourceDictionary
    {
        public PackageDisplayTemplates()
        {
            InitializeComponent();
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();

        private void PackageEnumerableGridView_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
                NavService.Navigate(e.AddedItems[0]);
        }
    }
}

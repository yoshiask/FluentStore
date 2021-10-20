using FluentStore.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;

namespace FluentStore.Themes
{
    public partial class PackageDisplayTemplates : ResourceDictionary
    {
        public PackageDisplayTemplates()
        {
            InitializeComponent();
        }

        private readonly INavigationService NavService = Ioc.Default.GetRequiredService<INavigationService>();

        private void PackageEnumerableGridView_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
                NavService.Navigate(e.AddedItems[0]);
        }
    }
}

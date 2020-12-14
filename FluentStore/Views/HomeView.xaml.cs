using FluentStore.ViewModels;
using MicrosoftStore.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class HomeView : Page
	{
		public ObservableCollection<ProductDetailsViewModel> CarouselItems { get; } = new ObservableCollection<ProductDetailsViewModel>();

		public HomeView()
		{
			this.InitializeComponent();
            Loaded += HomeView_Loaded;
		}

        private async void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
			var culture = CultureInfo.CurrentUICulture;
			var region = new RegionInfo(culture.LCID);

			var featured = await FluentStoreAPI.FluentStoreAPI.GetHomePageFeaturedAsync();
			CarouselItems.Clear();

			foreach (string productId in featured.Carousel)
            {
				var product = (await Apis.StorefrontApi.GetProduct(productId, region.TwoLetterISORegionName, culture.Name))
					.Convert<ProductDetails>().Payload;
				CarouselItems.Add(new ProductDetailsViewModel() { Product = product });
			}
			FeaturedCarousel.SelectedIndex = 1;
		}
    }
}

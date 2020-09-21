using MicrosoftStore.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace FluentStore.Controls
{
	public sealed class ProductCard : Control
	{
		public ProductDetails Details
		{
			get { return (ProductDetails)GetValue(DetailsProperty); }
			set { SetValue(DetailsProperty, value); }
		}
		public static readonly DependencyProperty DetailsProperty =
			DependencyProperty.Register("Details", typeof(ProductDetails), typeof(ProductCard), new PropertyMetadata(default(ProductDetails)));

		public ProductCard()
		{
			this.DefaultStyleKey = typeof(ProductCard);
		}

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ProductCard), new PropertyMetadata(null));
	}
}

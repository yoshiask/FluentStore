using FluentStore.ViewModels.Messages;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsView : Page
	{
		public SettingsView()
		{
			this.InitializeComponent();

			WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Settings"));
		}
	}
}

using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    public sealed partial class ProgressDialog : ContentDialog
    {
        public ProgressDialog()
        {
            this.InitializeComponent();
        }

        public async Task SetProgressAsync(double percent)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    IsIndeterminate = false;
                    Progress = (int)(percent * 100);
                });
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public string Body
        {
            get { return (string)GetValue(BodyProperty); }
            set { SetValue(BodyProperty, value); }
        }
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(nameof(Body), typeof(string), typeof(ProgressDialog), null);

        public int Progress
        {
            get => (int)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(int), typeof(ProgressDialog), new PropertyMetadata(0));

        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(ProgressDialog),
                new PropertyMetadata(true, UpdateProgressLabelVisibility));

        private static void UpdateProgressLabelVisibility(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dialog = (ProgressDialog)d;
            dialog.ProgressLabelBlock.Visibility = (bool)e.NewValue ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

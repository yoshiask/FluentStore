using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FluentStore.Controls
{
    public sealed partial class RatingBarControl : UserControl
    {
        public RatingBarControl()
        {
            this.InitializeComponent();
        }

        public int Star1Count
        {
            get => (int)GetValue(Star1CountProperty);
            set => SetValue(Star1CountProperty, value);
        }
        public static readonly DependencyProperty Star1CountProperty = DependencyProperty.Register(
            nameof(Star1Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0, OnStarCountChanged));

        public int Star2Count
        {
            get => (int)GetValue(Star2CountProperty);
            set => SetValue(Star2CountProperty, value);
        }
        public static readonly DependencyProperty Star2CountProperty = DependencyProperty.Register(
            nameof(Star2Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0, OnStarCountChanged));

        public int Star3Count
        {
            get => (int)GetValue(Star3CountProperty);
            set => SetValue(Star3CountProperty, value);
        }
        public static readonly DependencyProperty Star3CountProperty = DependencyProperty.Register(
            nameof(Star3Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0, OnStarCountChanged));

        public int Star4Count
        {
            get => (int)GetValue(Star4CountProperty);
            set => SetValue(Star4CountProperty, value);
        }
        public static readonly DependencyProperty Star4CountProperty = DependencyProperty.Register(
            nameof(Star4Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0, OnStarCountChanged));

        public int Star5Count
        {
            get => (int)GetValue(Star5CountProperty);
            set => SetValue(Star5CountProperty, value);
        }
        public static readonly DependencyProperty Star5CountProperty = DependencyProperty.Register(
            nameof(Star5Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0, OnStarCountChanged));

        public int TotalStarCount
        {
            get => (int)GetValue(TotalStarCountProperty);
            set => SetValue(TotalStarCountProperty, value);
        }
        public static readonly DependencyProperty TotalStarCountProperty = DependencyProperty.Register(
            nameof(TotalStarCount), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));

        private static void OnStarCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not RatingBarControl ratingBar)
                return;

            var total = ratingBar.Star1Count
                + ratingBar.Star2Count
                + ratingBar.Star3Count
                + ratingBar.Star4Count
                + ratingBar.Star5Count;
            ratingBar.TotalStarCount = Math.Max(total, 0);
        }
    }
}

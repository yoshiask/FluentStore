using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

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
            nameof(Star1Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));

        public int Star2Count
        {
            get => (int)GetValue(Star2CountProperty);
            set => SetValue(Star2CountProperty, value);
        }
        public static readonly DependencyProperty Star2CountProperty = DependencyProperty.Register(
            nameof(Star2Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));

        public int Star3Count
        {
            get => (int)GetValue(Star3CountProperty);
            set => SetValue(Star3CountProperty, value);
        }
        public static readonly DependencyProperty Star3CountProperty = DependencyProperty.Register(
            nameof(Star3Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));

        public int Star4Count
        {
            get => (int)GetValue(Star4CountProperty);
            set => SetValue(Star4CountProperty, value);
        }
        public static readonly DependencyProperty Star4CountProperty = DependencyProperty.Register(
            nameof(Star4Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));

        public int Star5Count
        {
            get => (int)GetValue(Star5CountProperty);
            set => SetValue(Star5CountProperty, value);
        }
        public static readonly DependencyProperty Star5CountProperty = DependencyProperty.Register(
            nameof(Star5Count), typeof(int), typeof(RatingBarControl), new PropertyMetadata(0));
    }
}

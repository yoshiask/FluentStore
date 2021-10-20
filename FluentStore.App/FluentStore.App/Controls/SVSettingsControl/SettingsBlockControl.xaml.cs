using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System;

namespace FluentStore.Controls
{
    [ContentProperty(Name = nameof(SettingsActionableElement))]
    public sealed partial class SettingsBlockControl : UserControl
    {
        public FrameworkElement SettingsActionableElement { get; set; }

        public FrameworkElement ExpandableContent
        {
            get => (FrameworkElement)GetValue(ExpandableContentProperty);
            set => SetValue(ExpandableContentProperty, value);
        }
        public static readonly DependencyProperty ExpandableContentProperty = DependencyProperty.Register(
            nameof(ExpandableContent), typeof(FrameworkElement), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public FrameworkElement AdditionalDescriptionContent
        {
            get => (FrameworkElement)GetValue(AdditionalDescriptionContentProperty);
            set => SetValue(AdditionalDescriptionContentProperty, value);
        }
        public static readonly DependencyProperty AdditionalDescriptionContentProperty = DependencyProperty.Register(
              nameof(AdditionalDescriptionContent), typeof(FrameworkElement), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
              nameof(Title), typeof(string), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
              nameof(Icon), typeof(IconElement), typeof(SettingsBlockControl), new PropertyMetadata(null));

        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }
        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
              nameof(IsClickable), typeof(bool), typeof(SettingsBlockControl), new PropertyMetadata(false));

        /// <summary>
        /// Occurs when a button control is clicked.
        /// </summary>
        public event RoutedEventHandler Click;

        public SettingsBlockControl()
        {
            this.InitializeComponent();
        }

        private void ActionableButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        // Microsoft.UI.Xaml.Controls.Expander was introduced in WinAppSDK 1.0.0-preview2
        private void Expander_Expanding(object? sender, EventArgs args)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }

        private void Expander_Collapsed(object? sender, EventArgs args)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FluentStore.Controls
{
    [TemplatePart(Name = TextBlockName, Type = typeof(TextBlock))]
    [TemplatePart(Name = MoreButtonName, Type = typeof(ButtonBase))]
    public sealed class TrimmedTextBlock : Control
    {
        private const string TextBlockName = "PART_TextBlock";
        private TextBlock TextBlock;
        private const string MoreButtonName = "PART_MoreButton";
        private ButtonBase MoreButton;

        public TrimmedTextBlock()
        {
            this.DefaultStyleKey = typeof(TrimmedTextBlock);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TrimmedTextBlock), new PropertyMetadata(string.Empty));

        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(object), typeof(TrimmedTextBlock), new PropertyMetadata("More"));

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }
        public static readonly DependencyProperty MaxLinesProperty = DependencyProperty.Register(
            nameof(MaxLines), typeof(int), typeof(TrimmedTextBlock), new PropertyMetadata(int.MaxValue));

        public HorizontalAlignment MoreButtonHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(MoreButtonHorizontalAlignmentProperty);
            set => SetValue(MoreButtonHorizontalAlignmentProperty, value);
        }
        public static readonly DependencyProperty MoreButtonHorizontalAlignmentProperty =
            DependencyProperty.Register(nameof(MoreButtonHorizontalAlignment), typeof(HorizontalAlignment), typeof(TrimmedTextBlock), new PropertyMetadata(HorizontalAlignment.Left));

        protected override void OnApplyTemplate()
        {
            TextBlock = GetTemplateChild(TextBlockName) as TextBlock;
            MoreButton = GetTemplateChild(MoreButtonName) as ButtonBase;

            if (MoreButton != null)
                MoreButton.Click += MoreButton_Click;
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Try to find a DisplayInfo object
            SDK.Attributes.DisplayInfo info = null;
            DependencyObject parent = this;
            const int maxDepth = 3;
            int curDepth = 0;
            while (info == null && curDepth <= maxDepth)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is FrameworkElement element && element.DataContext is SDK.Attributes.DisplayInfo foundInfo)
                {
                    info = foundInfo;
                    break;
                }
                curDepth++;
            }


            var dialog = new ContentDialog
            {
                Title = info?.Title ?? Title,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = Text,
                        TextWrapping = TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                    }
                },
                PrimaryButtonText = "Close",
                IsSecondaryButtonEnabled = false,
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}

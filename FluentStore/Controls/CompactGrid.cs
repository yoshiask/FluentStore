using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentStore.Controls
{
    public class CompactGrid : Grid
    {
        public const string DefaultCompactStateName = "Compact";
        public const string DefaultFullStateName = "Full";

        public string CompactStateName
        {
            get => (string)GetValue(CompactStateNameProperty);
            set => SetValue(CompactStateNameProperty, value);
        }
        public static readonly DependencyProperty CompactStateNameProperty = DependencyProperty.Register(
            nameof(CompactStateName), typeof(string), typeof(CompactGrid), new PropertyMetadata(DefaultCompactStateName));

        public string FullStateName
        {
            get => (string)GetValue(FullStateNameProperty);
            set => SetValue(FullStateNameProperty, value);
        }
        public static readonly DependencyProperty FullStateNameProperty = DependencyProperty.Register(
            nameof(FullStateName), typeof(string), typeof(CompactGrid), new PropertyMetadata(DefaultFullStateName));

        public static bool GetHideInCompact(DependencyObject obj)
        {
            return (bool)obj.GetValue(HideInCompactProperty);
        }
        public static void SetHideInCompact(DependencyObject obj, bool value)
        {
            obj.SetValue(HideInCompactProperty, value);
        }
        public static readonly DependencyProperty HideInCompactProperty = DependencyProperty.RegisterAttached(
            "HideInCompact", typeof(bool), typeof(CompactGrid), new PropertyMetadata(false));

        public static void SetVisualState(UIElement elem, string stateName)
        {
            if (elem is Panel pnl)
                foreach (UIElement subElem in pnl.Children)
                    SetVisualState(subElem, stateName);
            else if (typeof(Control).IsAssignableFrom(elem.GetType()))
                VisualStateManager.GoToState((Control)elem, stateName, true);
        }
        public void SetVisualState(string stateName) => SetVisualState(this, stateName);

        public static void SetCompactState(UIElement elem) => SetVisualState(elem, DefaultCompactStateName);
        public void SetCompactState() => SetVisualState(CompactStateName);

        public static void SetFullState(UIElement elem) => SetVisualState(elem, DefaultFullStateName);
        public void SetFullState() => SetVisualState(CompactStateName);
    }
}

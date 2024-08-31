using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentStore.Controls
{
    public class NavigationFrame : ContentControl
    {
        private readonly Stack<object> m_navStack = new();

        public bool CanGoBack => m_navStack.Count > 0;

        public bool CanGoForward => false;

        public Type CurrentPageType { get; private set; }

        public void Navigate(Type type, object parameter = null)
        {
            var page = type.GetConstructor(Type.EmptyTypes).Invoke(null) as UIElement;
            Navigate(page, parameter, type);
        }

        public void Navigate(UIElement newContent, object parameter = null, Type sourceType = null)
        {
            var oldContent = (UIElement)Content;
            System.Diagnostics.Debug.WriteLineIf(newContent == oldContent, "tried to navigate to the same page");
            if (oldContent is not null)
            {
                m_navStack.Push(oldContent);
                if (TryGetContent(out var oldAppContent))
                    oldAppContent.OnNavigatedFrom(parameter);
            }

            Content = newContent;
            if (TryGetContent(out var newAppContent))
                newAppContent.OnNavigatedTo(parameter);

            CurrentPageType = sourceType ?? Content.GetType();
            Navigated?.Invoke(this, Content);
        }

        public bool NavigateBack()
        {
            if (m_navStack.TryPop(out var oldContent))
            {
                if (TryGetContent(out var newAppContent))
                    newAppContent.OnNavigatedFrom(null);

                Content = oldContent;
                if (TryGetContent(out var oldAppContent))
                    oldAppContent.OnNavigatedTo(null);

                CurrentPageType = Content.GetType();
                Navigated?.Invoke(this, Content);

                return true;
            }
            return false;
        }

        public bool NavigateForward() => throw new NotImplementedException();

        public bool TryGetContent(out IAppContent content)
        {
            content = Content as IAppContent;
            return content != null;
        }

        public void ClearNavigationStack() => m_navStack.Clear();

        public event EventHandler<object> Navigated;
    }
}

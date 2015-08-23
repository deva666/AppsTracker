using System;
using System.Windows;

namespace AppsTracker.Widgets.Utils
{
    public sealed class WindowClose
    {
        public static DependencyProperty WindowCloseProperty =
                            DependencyProperty.RegisterAttached(
                            "WindowClose",
                            typeof(Boolean),
                            typeof(WindowClose),
                            new PropertyMetadata(WindowCloseChanged));

        private static void WindowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null)
                return;

            var closing = (Boolean)e.NewValue;
            if (closing)
                window.Close();
        }

        public static void SetWindowClose(Window window, Boolean value)
        {
            window.SetValue(WindowCloseProperty, value);
        }

        public static Boolean GetWindowClose(Window window)
        {
            return (Boolean)window.GetValue(WindowCloseProperty);
        }
    }
}

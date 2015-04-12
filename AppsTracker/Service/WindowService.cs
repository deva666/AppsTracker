using System;
using System.ComponentModel.Composition;
using AppsTracker.Widgets;

namespace AppsTracker.Service
{
    [Export(typeof(IWindowService))]
    internal sealed class WindowService : AppsTracker.Service.IWindowService
    {
        public void ShowDialog(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.ShowDialog();
        }

        public void ShowDialog(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.ShowDialog();
        }

        public void Show(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.Show();
        }

        public void Show(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.Show();
        }


        public void ShowWindow<T>() where T : System.Windows.Window
        {
            var instance = Activator.CreateInstance<T>();            
            instance.Show();
        }


        public void ShowWindow<T>(params object[] args) where T : System.Windows.Window
        {
            T instance = (T)Activator.CreateInstance(typeof(T), args);
            instance.Show();
        }
    }
}

using System;
using System.ComponentModel.Composition;
using AppsTracker.Widgets;

namespace AppsTracker.Service
{
    [Export(typeof(IMessageService))]
    internal sealed class MessageService : AppsTracker.Service.IMessageService
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
    }
}

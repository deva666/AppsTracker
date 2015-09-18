#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Service;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows.Forms;

namespace AppsTracker.Widgets
{
    [Export(typeof(ITrayIcon))]
    public class TrayIcon : IDisposable, ITrayIcon
    {
        [Import]
        private Lazy<IWindowService> windowService = null;

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip iconMenu;
        private readonly ToolStripMenuItem menuItemShowApp;
        private readonly ToolStripMenuItem menuItemExit;

        public bool IsVisible
        {
            get { return notifyIcon.Visible; }
            set { notifyIcon.Visible = value; }
        }


        public TrayIcon()
        {
            notifyIcon = new NotifyIcon();
            iconMenu = new ContextMenuStrip();
            menuItemShowApp = new ToolStripMenuItem(string.Format("Open {0}", Constants.APP_NAME));
            menuItemShowApp.Click += (s, e) => windowService.Value.CreateOrShowMainWindow();
            menuItemExit = new ToolStripMenuItem("Exit");
            iconMenu.Items.Add(menuItemShowApp);
            iconMenu.Items.Add(menuItemExit);
            notifyIcon.ContextMenuStrip = iconMenu;
            notifyIcon.Icon = Properties.Resources.icon1;
            notifyIcon.Text = Constants.APP_NAME;
            notifyIcon.Visible = true;

            notifyIcon.Click += (s, e) =>
            {
                notifyIcon.GetType().InvokeMember("ShowContextMenu",
                    BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, notifyIcon, null);
            };
            notifyIcon.DoubleClick += (s, e) => windowService.Value.CreateOrShowMainWindow();
            menuItemExit.Click += (s, e) => ((App)App.Current).ShutdownApp();

            IsVisible = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                menuItemExit.Dispose();
                menuItemShowApp.Dispose();
                iconMenu.Dispose();
                notifyIcon.Dispose();
            }
        }
    }
}

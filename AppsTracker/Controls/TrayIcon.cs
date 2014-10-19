using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AppsTracker.Controls
{
    public class TrayIcon : IDisposable
    {
        NotifyIcon notifyIcon;
        ContextMenuStrip iconMenu;
        ToolStripMenuItem menuItemShowApp;
        ToolStripMenuItem menuItemEnterStealth;
        ToolStripMenuItem menuItemExit;

        public bool IsVisible { get { return notifyIcon.Visible; } set { notifyIcon.Visible = value; } }

        #region Constructor

        public TrayIcon()
        {
            notifyIcon = new NotifyIcon();
            iconMenu = new ContextMenuStrip();
            menuItemShowApp = new ToolStripMenuItem( string.Format("Open {0}", Constants.APP_NAME) );
            menuItemEnterStealth = new ToolStripMenuItem( "Enter stealth mode" );
            menuItemExit = new ToolStripMenuItem( "Exit" );
            iconMenu.Items.Add( menuItemShowApp );
            iconMenu.Items.Add( menuItemExit );
            notifyIcon.ContextMenuStrip = iconMenu;
            notifyIcon.Icon = Properties.Resources.icon1;
            notifyIcon.Text = Constants.APP_NAME;
            notifyIcon.Visible = true;

            #region Event Handlers

            menuItemExit.Click += ( s, e ) => { ( App.Current as App ).FinishAndExit(true); };
            menuItemShowApp.Click += ( s, e ) => { ( App.Current as App ).CreateOrShowMainWindow(); };

            #endregion
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( menuItemEnterStealth != null ) menuItemEnterStealth.Dispose();
                if ( menuItemExit != null ) menuItemExit.Dispose();
                if ( menuItemShowApp != null ) menuItemShowApp.Dispose();
                if ( iconMenu != null ) iconMenu.Dispose();
                if ( notifyIcon != null ) notifyIcon.Dispose();
            }
        }

        #endregion
    }
}

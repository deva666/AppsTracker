using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Task_Logger_Pro.Views
{
    public partial class Settings_generalView : UserControl
    {
        public Settings_generalView()
        {
            InitializeComponent();
        }

        private void popupMasterPassword_Opened(object sender, EventArgs e)
        {
            if ( App.DataLogger.KeyBoardHook != null)
                App.DataLogger.KeyBoardHook.KeyLoggerEnabled = false;
        }

        private void popupMasterPassword_Closed(object sender, EventArgs e)
        {
            if ( App.UzerSetting.EnableKeylogger && App.DataLogger.KeyBoardHook != null) 
                App.DataLogger.KeyBoardHook.KeyLoggerEnabled = true;
        }

        private void popupEmailInterval_Opened( object sender, EventArgs e )
        {
            if (App.DataLogger.KeyBoardHook != null)
                App.DataLogger.KeyBoardHook.KeyLoggerEnabled = false;
        }

        private void popupEmailInterval_Closed( object sender, EventArgs e )
        {
            if (App.UzerSetting.EnableKeylogger && App.DataLogger.KeyBoardHook != null)
                App.DataLogger.KeyBoardHook.KeyLoggerEnabled = true;
        }

    }
}

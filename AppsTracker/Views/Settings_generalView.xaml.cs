using System;
using System.Windows.Controls;

namespace AppsTracker.Views
{
    public partial class Settings_generalView : UserControl
    {
        public Settings_generalView()
        {
            InitializeComponent();
        }

        private void popupMasterPassword_Opened(object sender, EventArgs e)
        {
            SetKeylogger(false);
        }

        private void popupMasterPassword_Closed(object sender, EventArgs e)
        {
            SetKeylogger(true);
        }

        private void popupEmailInterval_Opened(object sender, EventArgs e)
        {
            SetKeylogger(false);
        }

        private void popupEmailInterval_Closed(object sender, EventArgs e)
        {
            SetKeylogger(true);
        }

        private void SetKeylogger(bool enabled)
        {
            //if (App.UzerSetting.LoggingEnabled && App.UzerSetting.EnableKeylogger)
            //    App.Container.SetKeyboardHookEnabled(enabled);
        }

    }
}

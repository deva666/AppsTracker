using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_Logger_Pro.Models
{
    public partial class Setting
    {
        public ScreenShotInterval ScreenshotInterval
        {
            get { return this.TimerInterval.ConvertToScreenshotInterval(); }
            set { this.TimerInterval = value.ConvertToDouble(); }
        }

        public EmailInterval EmailIntervalEnum
        {
            get { return this.EmailInterval.ConvertToEmailInterval(); }
            set { this.EmailInterval = value.CovertToDouble(); }
        }

        public Setting() { }

        public Setting(bool first)
            : this()
        {
            if (first)
            {
                SetInitialSettings();

            }
        }

        private void SetInitialSettings()
        {
            this.DeleteOldLogs = true;
            this.EmailFrom = "";
            this.EmailInterval = 30 * 60 * 1000;
            this.EmailSmtpHost = "";
            this.EmailSmtpPassword = "";
            this.EmailSmtpPort = 0;
            this.EmailSmtpUsername = "";
            this.EmailSSL = false;
            this.EmailTo = "";
            this.EnableEmailReports = false;
            this.EnableFileWatcher = false;
            this.EnableKeylogger = true;
            this.FileWatcherPath = @"C:\";
            this.FileWatcherSubdirectories = true;
            this.IsMasterPasswordSet = false;
            this.Licence = false;
            this.LightTheme = true;
            this.LoggingEnabled = true;
            this.MainWindowHeight = 800;
            this.MainWindowLeft = 250;
            this.MainWindowTop = 250;
            this.MainWindowWidth = 1000;
            this.OldLogDeleteDays = 30;
            this.RunAtStartup = false;
            this.Stealth = false;
            this.TakeScreenshots = true;
            this.TimerInterval = 5 * 60 * 1000;
            this.WindowOpen = "";
            this.TrialStartDate = DateTime.Now;
            this.EnableIdle = true;
            this.IdleTimer = 2 * 60 * 1000;
            this.FirstRun = true;
        }

    }
}

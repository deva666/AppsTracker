#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;

namespace AppsTracker.Models.Proxy
{
    public sealed class DummySettings : ISettings
    {
        public bool Licence
        {
            get;
            set;
        }

        public bool RunAtStartup
        {
            get;
            set;
        }

        public bool EnableKeylogger
        {
            get;
            set;
        }

        public bool TakeScreenshots
        {
            get;
            set;
        }

        public bool Stealth
        {
            get;
            set;
        }

        public bool EnableFileWatcher
        {
            get;
            set;
        }

        public bool FileWatcherSubdirectories
        {
            get;
            set;
        }

        public bool IsMasterPasswordSet
        {
            get;
            set;
        }

        public bool DeleteOldLogs
        {
            get;
            set;
        }

        public bool EnableEmailReports
        {
            get;
            set;
        }

        public bool EmailSSL
        {
            get;
            set;
        }

        public bool LightTheme
        {
            get;
            set;
        }

        public bool FirstRun
        {
            get;
            set;
        }

        public bool LoggingEnabled
        {
            get;
            set;
        }

        public int EmailSmtpPort
        {
            get;
            set;
        }

        public string EmailTo
        {
            get;
            set;
        }

        public string EmailFrom
        {
            get;
            set;
        }

        public string EmailSmtpHost
        {
            get;
            set;
        }

        public string EmailSmtpUsername
        {
            get;
            set;
        }

        public string EmailSmtpPassword
        {
            get;
            set;
        }

        public double EmailInterval
        {
            get;
            set;
        }

        public double TimerInterval
        {
            get;
            set;
        }

        public string FileWatcherPath
        {
            get;
            set;
        }

        public short OldLogDeleteDays
        {
            get;
            set;
        }

        public double MainWindowLeft
        {
            get;
            set;
        }

        public double MainWindowTop
        {
            get;
            set;
        }

        public double MainWindowWidth
        {
            get;
            set;
        }

        public double MainWindowHeight
        {
            get;
            set;
        }

        public string WindowOpen
        {
            get;
            set;
        }

        public DateTime TrialStartDate
        {
            get;
            set;
        }

        public bool EnableIdle
        {
            get;
            set;
        }

        public long IdleTimer
        {
            get;
            set;
        }

        public double ScreenshotWindowWidth
        {
            get;
            set;
        }

        public double ScreenshotWindowHeight
        {
            get;
            set;
        }

        public double ScreenshotWindowTop
        {
            get;
            set;
        }

        public double ScreenshotWindowLeft
        {
            get;
            set;
        }

        public DateTime? LastExecutedDate
        {
            get;
            set;
        }

        public string DefaultScreenshotSavePath
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }
    }
}

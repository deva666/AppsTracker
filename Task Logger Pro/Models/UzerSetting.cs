using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task_Logger_Pro.MVVM;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro
{
    public sealed class UzerSetting : ObservableObject
    {
        public bool Licence { get { return App.Settings.Licence; } set { App.Settings.Licence = value; PropertyChanging("Licence"); } }
        public bool RunAtStartup { get { return App.Settings.RunAtStartup; } set { App.Settings.RunAtStartup = value; PropertyChanging("RunAtStartup"); } }
        public bool EnableKeylogger { get { return App.Settings.EnableKeylogger; } set { App.Settings.EnableKeylogger = value; PropertyChanging("EnableKeylogger"); } }
        public bool TakeScreenshots { get { return App.Settings.TakeScreenshots; } set { App.Settings.TakeScreenshots = value; PropertyChanging("TakeScreenshots"); } }
        public bool Stealth { get { return App.Settings.Stealth; } set { App.Settings.Stealth = value; PropertyChanging("Stealth"); } }
        public bool EnableFileWatcher { get { return App.Settings.EnableFileWatcher; } set { App.Settings.EnableFileWatcher = value; PropertyChanging("EnableFileWatcher"); } }
        public bool FileWatcherSubdirectories { get { return App.Settings.FileWatcherSubdirectories; } set { App.Settings.FileWatcherSubdirectories = value; PropertyChanging("FileWatcherSubdirectories"); } }
        public bool IsMasterPasswordSet { get { return App.Settings.IsMasterPasswordSet; } set { App.Settings.IsMasterPasswordSet = value; PropertyChanging("IsMasterPasswordSet"); } }
        public bool DeleteOldLogs { get { return App.Settings.DeleteOldLogs; } set { App.Settings.DeleteOldLogs = value; PropertyChanging("DeleteOldLogs"); } }
        public bool EnableEmailReports { get { return App.Settings.EnableEmailReports; } set { App.Settings.EnableEmailReports = value; PropertyChanging("EnableEmailReports"); } }
        public bool EmailSSL { get { return App.Settings.EmailSSL; } set { App.Settings.EmailSSL = value; PropertyChanging("EmailSSL"); } }
        public bool LightTheme { get { return App.Settings.LightTheme; } set { App.Settings.LightTheme = value; PropertyChanging("LightTheme"); } }
        public bool LoggingEnabled { get { return App.Settings.LoggingEnabled; } set { App.Settings.LoggingEnabled = value; PropertyChanging("LoggingEnabled"); } }
        public int EmailSmtpPort { get { return App.Settings.EmailSmtpPort; } set { App.Settings.EmailSmtpPort = value; PropertyChanging("EmailSmtpPort"); } }
        public string EmailTo { get { return App.Settings.EmailTo; } set { App.Settings.EmailTo = value; PropertyChanging("EmailTo"); } }
        public string EmailFrom { get { return App.Settings.EmailFrom; } set { App.Settings.EmailFrom = value; PropertyChanging("EmailFrom"); } }
        public string EmailSmtpHost { get { return App.Settings.EmailSmtpHost; } set { App.Settings.EmailSmtpHost = value; PropertyChanging("EmailSmtpHost"); } }
        public string EmailSmtpUsername { get { return App.Settings.EmailSmtpUsername; } set { App.Settings.EmailSmtpUsername = value; PropertyChanging("EmailSmtpUsername"); } }
        public string EmailSmtpPassword { get { return App.Settings.EmailSmtpPassword; } set { App.Settings.EmailSmtpPassword = value; PropertyChanging("EmailSmtpPassword"); } }
        public double EmailInterval { get { return App.Settings.EmailInterval; } set { App.Settings.EmailInterval = value; PropertyChanging("EmailInterval"); } }
        public double TimerInterval { get { return App.Settings.TimerInterval; } set { App.Settings.TimerInterval = value; PropertyChanging("TimerInterval"); } }
        public string FileWatcherPath { get { return App.Settings.FileWatcherPath; } set { App.Settings.FileWatcherPath = value; PropertyChanging("FileWatcherPath"); } }
        public short OldLogDeleteDays { get { return App.Settings.OldLogDeleteDays; } set { App.Settings.OldLogDeleteDays = value; PropertyChanging("OldLogDeleteDays"); } }
        public double MainWindowLeft { get { return App.Settings.MainWindowLeft; } set { App.Settings.MainWindowLeft = value; PropertyChanging("MainWindowLeft"); } }
        public double MainWindowTop { get { return App.Settings.MainWindowTop; } set { App.Settings.MainWindowTop = value; PropertyChanging("MainWindowTop"); } }
        public double MainWindowWidth { get { return App.Settings.MainWindowWidth; } set { App.Settings.MainWindowWidth = value; PropertyChanging("MainWindowWidth"); } }
        public double MainWindowHeight { get { return App.Settings.MainWindowHeight; } set { App.Settings.MainWindowHeight = value; PropertyChanging("MainWindowHeight"); } }
        public string WindowOpen { get { return App.Settings.WindowOpen; } set { App.Settings.WindowOpen = value; PropertyChanging("WindowOpen"); } }
        public bool FirstRun { get { return App.Settings.FirstRun; } set { App.Settings.FirstRun = value; PropertyChanging("FirstRun"); } }
        public EmailInterval EmailIntervalEnum { get { return App.Settings.EmailIntervalEnum; } set { App.Settings.EmailIntervalEnum = value; PropertyChanging("EmailIntervalEnum"); } }
        public ScreenShotInterval ScreenshotInterval { get { return App.Settings.ScreenshotInterval; } set { App.Settings.ScreenshotInterval = value; PropertyChanging("ScreenshotInterval"); } }
        public DateTime TrialStartDate { get { return App.Settings.TrialStartDate; } set { App.Settings.TrialStartDate = value; PropertyChanging("TrialStartDate"); } }
        public bool EnableIdle { get { return App.Settings.EnableIdle; } set { App.Settings.EnableIdle = value;  PropertyChanging("EnableIdle");} }
        public long IdleInterval { get { return App.Settings.IdleTimer; } set { App.Settings.IdleTimer = value;  PropertyChanging("IdleInterval");} }
        public double ScreenshotWindowLeft { get { return App.Settings.ScreenshotWindowLeft; } set { App.Settings.ScreenshotWindowLeft = value; PropertyChanging("ScreenshotWindowLeft"); } }
        public double ScreenshotWindowTop { get { return App.Settings.MainWindowTop; } set { App.Settings.ScreenshotWindowTop = value; PropertyChanging("ScreenshotWindowTop"); } }
        public double ScreenshotWindowWidth { get { return App.Settings.ScreenshotWindowWidth; } set { App.Settings.ScreenshotWindowWidth = value; PropertyChanging("ScreenshotWindowWidth"); } }
        public double ScreenshotWindowHeight { get { return App.Settings.ScreenshotWindowHeight; } set { App.Settings.ScreenshotWindowHeight = value; PropertyChanging("ScreenshotWindowHeight"); } }
        public DateTime? LastExecutedDate { get { return App.Settings.LastExecutedDate; } set { App.Settings.LastExecutedDate = value; PropertyChanging("LastExecutedDate"); } }
        public string Username { get { return App.Settings.Username; } set { App.Settings.Username = value; PropertyChanging("Username"); } }
        public string DefaultScreenshotSavePath { get { return App.Settings.DefaultScreenshotSavePath; } set { App.Settings.DefaultScreenshotSavePath = value; PropertyChanging("DefaultScreenshotSavePath"); } }
    }
}

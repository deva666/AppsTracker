using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Models.Proxy
{
    public interface ISettings
    {
        bool Licence { get; set; }
        bool RunAtStartup { get; set; }
        bool EnableKeylogger { get; set; }
        bool TakeScreenshots { get; set; }
        bool Stealth { get; set; }
        bool EnableFileWatcher { get; set; }
        bool FileWatcherSubdirectories { get; set; }
        bool IsMasterPasswordSet { get; set; }
        bool DeleteOldLogs { get; set; }
        bool EnableEmailReports { get; set; }
        bool EmailSSL { get; set; }
        bool LightTheme { get; set; }
        bool FirstRun { get; set; }
        bool LoggingEnabled { get; set; }
        int EmailSmtpPort { get; set; }
        string EmailTo { get; set; }
        string EmailFrom { get; set; }
        string EmailSmtpHost { get; set; }
        string EmailSmtpUsername { get; set; }
        string EmailSmtpPassword { get; set; }
        double EmailInterval { get; set; }
        double TimerInterval { get; set; }
        string FileWatcherPath { get; set; }
        short OldLogDeleteDays { get; set; }
        double MainWindowLeft { get; set; }
        double MainWindowTop { get; set; }
        double MainWindowWidth { get; set; }
        double MainWindowHeight { get; set; }
        string WindowOpen { get; set; }
        System.DateTime TrialStartDate { get; set; }
        bool EnableIdle { get; set; }
        long IdleTimer { get; set; }
        double ScreenshotWindowWidth { get; set; }
        double ScreenshotWindowHeight { get; set; }
        double ScreenshotWindowTop { get; set; }
        double ScreenshotWindowLeft { get; set; }
        Nullable<System.DateTime> LastExecutedDate { get; set; }
        string DefaultScreenshotSavePath { get; set; }
        string Username { get; set; }
    }
}

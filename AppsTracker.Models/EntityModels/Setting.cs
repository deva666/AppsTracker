#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using AppsTracker.Models.Proxy;

namespace AppsTracker.Models.EntityModels
{
    public enum ScreenShotInterval : uint
    {
        [Description("10 sec")]
        TenSeconds = 10000,
        [Description("30 sec")]
        ThirtySeconds = 30000,
        [Description("1 min")]
        OneMinute = 60000,
        [Description("2 min")]
        TwoMinute = 120000,
        [Description("5 min")]
        FiveMinute = 300000,
        [Description("10 min")]
        TenMinute = 600000,
        [Description("30 min")]
        ThirtyMinute = 1800000,
        [Description("1 hr")]
        OneHour = 3600000
    }

    public enum EmailInterval : uint
    {
        [Description("5 min")]
        FiveMinute = 300000,
        [Description("10 min")]
        TenMinute = 600000,
        [Description("30 min")]
        ThirtyMinute = 1800000,
        [Description("1 hr")]
        OneHour = 3600000,
        [Description("2 hr")]
        TwoHour = 7200000
    }

    public class Setting : ISettings
    {
        [NotMapped]
        public ScreenShotInterval ScreenshotInterval
        {
            get { return ((ScreenShotInterval)Enum.Parse(typeof(ScreenShotInterval), TimerInterval.ToString())); }
            set { this.TimerInterval = (double)value; }
        }

        [NotMapped]
        public EmailInterval EmailIntervalEnum
        {
            get { return ((EmailInterval)Enum.Parse(typeof(EmailInterval), EmailInterval.ToString())); }
            set { this.EmailInterval = (double)value; }
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
            this.EmailFrom = "";
            this.EmailInterval = 1800000;
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
            this.DeleteOldLogs = true;
            this.OldLogDeleteDays = 15;
            this.RunAtStartup = false;
            this.Stealth = false;
            this.TakeScreenshots = true;
            this.TimerInterval = 2 * 60 * 1000;
            this.WindowOpen = "";
            this.TrialStartDate = DateTime.Now;
            this.EnableIdle = true;
            this.IdleTimer = 2 * 60 * 1000;
            this.FirstRun = true;
            if (CreateScreenshotFolder())
                this.DefaultScreenshotSavePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Screenshots");
            else
                this.DefaultScreenshotSavePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private bool CreateScreenshotFolder()
        {
            try
            {
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Screenshots")))
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Screenshots"));
                return true;
            }
            catch
            {
                return false;
            }
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingsID { get; set; }

        [Required]
        public bool Licence { get; set; }

        [Required]
        public bool RunAtStartup { get; set; }

        [Required]
        public bool EnableKeylogger { get; set; }

        [Required]
        public bool TakeScreenshots { get; set; }

        [Required]
        public bool Stealth { get; set; }

        [Required]
        public bool EnableFileWatcher { get; set; }

        [Required]
        public bool FileWatcherSubdirectories { get; set; }

        [Required]
        public bool IsMasterPasswordSet { get; set; }

        [Required]
        public bool DeleteOldLogs { get; set; }


        public bool EnableEmailReports { get; set; }


        public bool EmailSSL { get; set; }


        [Required]
        public bool LightTheme { get; set; }

        [Required]
        public bool FirstRun { get; set; }

        [Required]
        public bool LoggingEnabled { get; set; }


        public int EmailSmtpPort { get; set; }


        [StringLength(60)]
        [DefaultValue("")]
        public string EmailTo { get; set; }


        [StringLength(60)]
        [DefaultValue("")]
        public string EmailFrom { get; set; }


        [StringLength(30)]
        public string EmailSmtpHost { get; set; }


        [StringLength(60)]
        [DefaultValue("")]
        public string EmailSmtpUsername { get; set; }


        [StringLength(60)]
        [DefaultValue("")]
        public string EmailSmtpPassword { get; set; }

        [Required]
        public double EmailInterval { get; set; }

        [Required]
        public double TimerInterval { get; set; }

        [Required]
        [StringLength(360)]
        public string FileWatcherPath { get; set; }

        [Required]
        public short OldLogDeleteDays { get; set; }

        [Required]
        public double MainWindowLeft { get; set; }

        [Required]
        public double MainWindowTop { get; set; }

        [Required]
        public double MainWindowWidth { get; set; }

        [Required]
        public double MainWindowHeight { get; set; }

        [StringLength(64)]
        public string WindowOpen { get; set; }

        [Required]
        public System.DateTime TrialStartDate { get; set; }

        [Required]
        public bool EnableIdle { get; set; }

        [Required]
        public long IdleTimer { get; set; }

        [Required]
        public double ScreenshotWindowWidth { get; set; }

        [Required]
        public double ScreenshotWindowHeight { get; set; }

        [Required]
        public double ScreenshotWindowTop { get; set; }

        [Required]
        public double ScreenshotWindowLeft { get; set; }

        public Nullable<System.DateTime> LastExecutedDate { get; set; }

        [Required]
        [StringLength(360)]
        public string DefaultScreenshotSavePath { get; set; }


        [StringLength(60)]
        public string Username { get; set; }
    }
}

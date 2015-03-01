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

namespace AppsTracker.Data.Models
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

    public class Setting 
    {
        [NotMapped]
        public ScreenShotInterval ScreenshotInterval
        {
            get { return ((ScreenShotInterval)Enum.Parse(typeof(ScreenShotInterval), TimerInterval.ToString())); }
            set { this.TimerInterval = (double)value; }
        }

        [NotMapped]
        public bool IsSelected { get; set; }

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
            this.EnableKeylogger = false;
            this.IsMasterPasswordSet = false;
            this.LightTheme = true;
            this.LoggingEnabled = true;
            this.DeleteOldLogs = true;
            this.OldLogDeleteDays = 15;
            this.RunAtStartup = false;
            this.TakeScreenshots = true;
            this.TimerInterval = 2 * 60 * 1000;
            this.WindowOpen = "";
            this.EnableIdle = true;
            this.IdleTimer = 2 * 60 * 1000;
            this.FirstRun = true;
            if (ScreenshotFolderCreated())
                this.DefaultScreenshotSavePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Screenshots");
            else
                this.DefaultScreenshotSavePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private bool ScreenshotFolderCreated()
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

        public Setting Clone()
        {
            return (Setting)this.MemberwiseClone();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingsID { get; set; }

        [Required]
        public bool RunAtStartup { get; set; }

        [Required]
        public bool EnableKeylogger { get; set; }

        [Required]
        public bool TakeScreenshots { get; set; }


        [Required]
        public bool IsMasterPasswordSet { get; set; }

        [Required]
        public bool DeleteOldLogs { get; set; }


        [Required]
        public bool LightTheme { get; set; }

        [Required]
        public bool FirstRun { get; set; }

        [Required]
        public bool LoggingEnabled { get; set; }

        [Required]
        public double TimerInterval { get; set; }

        [Required]
        public short OldLogDeleteDays { get; set; }

        [StringLength(64)]
        public string WindowOpen { get; set; }

        [Required]
        public bool EnableIdle { get; set; }

        [Required]
        public long IdleTimer { get; set; }

        [Required]
        [StringLength(360)]
        public string DefaultScreenshotSavePath { get; set; }
    }
}

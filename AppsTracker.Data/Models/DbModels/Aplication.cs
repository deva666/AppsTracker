#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppsTracker.Data.Utils;

namespace AppsTracker.Data.Models
{
    public class Aplication
    {
        [NotMapped]
        public TimeSpan Duration
        {
            get
            {
                return GetAppDuration();
            }
        }


        [NotMapped]
        public ObservableCollection<AppLimit> ObservableLimits { get; set; }


        private TimeSpan GetAppDuration()
        {
            long ticks = 0;
            foreach (var window in this.Windows)
            {
                foreach (var log in window.Logs)
                {
                    ticks += log.Duration;
                }
            }

            return new TimeSpan(ticks);
        }


        public Aplication() { }


        public Aplication(string name, string fileName, string version, string description, string company, string realName)
        {
            this.BlockedApps = new HashSet<BlockedApp>();
            this.Windows = new HashSet<Window>();
            this.Categories = new HashSet<AppCategory>();
            this.Limits = new HashSet<AppLimit>();

            this.Name = !string.IsNullOrEmpty(name) ? name.Truncate(250) : !string.IsNullOrEmpty(realName) ? realName.Truncate(250) : fileName.Truncate(250);
            this.FileName = fileName.Truncate(360);
            this.Version = version.Truncate(50);
            this.Description = description.Truncate(150);
            this.Company = company.Truncate(150);
            this.WinName = realName.Truncate(100);
        }


        public Aplication(IAppInfo appInfo)
            : this(appInfo.Name, appInfo.FileName, appInfo.Version, appInfo.Description, appInfo.Company, appInfo.FullName)
        {

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationID { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [StringLength(360)]
        public string FileName { get; set; }

        [StringLength(50)]
        public string Version { get; set; }

        [StringLength(150)]
        public string Description { get; set; }

        [StringLength(150)]
        public string Company { get; set; }

        [Required]
        public int UserID { get; set; }

        [StringLength(100)]
        public string WinName { get; set; }

        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
        public virtual ICollection<BlockedApp> BlockedApps { get; set; }
        public virtual ICollection<Window> Windows { get; set; }
        public virtual ICollection<AppCategory> Categories { get; set; }
        public virtual ICollection<AppLimit> Limits { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Aplication))
                return false;

            var comparingApp = (Aplication)obj;
            return this.ApplicationID == comparingApp.ApplicationID && this.UserID == comparingApp.UserID;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + ApplicationID.GetHashCode();
                hash = hash * 23 + UserID.GetHashCode();
                return hash;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppsTracker.Models.Utils;

namespace AppsTracker.Models.EntityModels
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

        public Aplication()
            : this("", "", "", "", "", "")
        {
            this.FileName = "";
            this.Description = "";
            this.Company = "";
            this.Version = "";
            this.WinName = "";
        }

        public Aplication(string name, string fileName, string version, string description, string company, string realName)
        {
            this.AppsToBlocks = new HashSet<AppsToBlock>();
            this.BlockedApps = new HashSet<BlockedApp>();
            this.Windows = new HashSet<Window>();

            this.Name = name.Truncate(250);
            this.FileName = fileName.Truncate(360);
            this.Version = version.Truncate(50);
            this.Description = description.Truncate(150);
            this.Company = company.Truncate(150);
            this.WinName = realName.Truncate(100);
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
        public virtual ICollection<AppsToBlock> AppsToBlocks { get; set; }
        public virtual ICollection<BlockedApp> BlockedApps { get; set; }
        public virtual ICollection<Window> Windows { get; set; }
    }
}

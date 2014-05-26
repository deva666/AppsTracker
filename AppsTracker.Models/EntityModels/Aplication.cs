using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        {
            this.AppsToBlocks = new HashSet<AppsToBlock>();
            this.BlockedApps = new HashSet<BlockedApp>();
            this.Windows = new HashSet<Window>();

            this.FileName = "";
            this.Description = "";
            this.Company = "";
            this.Version = "";
            this.WinName = "";
        }

        public Aplication(string name, string fileName, string version, string description, string company, string realName)
        {
            this.Name = name;
            this.FileName = fileName;
            this.Version = version;
            this.Description = description;
            this.Company = company;
            this.WinName = realName;
        }


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(360)]
        public string FileName { get; set; }

        [Required]
        [StringLength(30)]
        public string Version { get; set; }

        [Required]
        [StringLength(150)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Company { get; set; }
        
        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string WinName { get; set; }

        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
        public virtual ICollection<AppsToBlock> AppsToBlocks { get; set; }
        public virtual ICollection<BlockedApp> BlockedApps { get; set; }
        public virtual ICollection<Window> Windows { get; set; }
    }
}

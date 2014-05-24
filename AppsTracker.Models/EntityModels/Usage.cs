using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace AppsTracker.Models.EntityModels
{
    public class Usage
    {
        [NotMapped]
        public bool IsSelected { get; set; }

        [NotMapped]
        public TimeSpan Duration
        {
            get
            {
                return IsCurrent ? new TimeSpan(DateTime.Now.Ticks - UsageStart.Ticks) : new TimeSpan(UsageEnd.Ticks - UsageStart.Ticks);
            }
        }

        public Usage(int userID)
            : this()
        {
            this.UsageStart = DateTime.Now;
            this.UserID = userID;
        }

        public Usage(int userID, int usageTypeID)
            : this(userID)
        {
            this.UsageTypeID = usageTypeID;
        }

        public Usage()
        {
            //this.Logs = new HashSet<Log>();
            //this.SelfUsages = new HashSet<Usage>();
            //this.SelfUsage = new HashSet<Usage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsageID { get; set; }

        [Required]
        public int UsageTypeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public System.DateTime UsageStart { get; set; }

        [Required]
        public System.DateTime UsageEnd { get; set; }

        [Required]
        public bool IsCurrent { get; set; }
        public Nullable<int> SelfUsageID { get; set; }

        [ForeignKey("UsageTypeID")]
        public virtual UsageType UsageType { get; set; }

        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
        public virtual ICollection<Log> Logs { get; set; }
        public virtual ICollection<Usage> SelfUsages { get; set; }
        public virtual ICollection<Usage> SelfUsage { get; set; }
    }
}

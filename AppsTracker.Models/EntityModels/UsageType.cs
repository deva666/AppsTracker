using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AppsTracker.Models.EntityModels
{
    public enum UsageTypes : byte
    {
        Login,
        Idle,
        Locked,
        Stopped
    }

    public class UsageType
    {
        public UsageType()
        {
            this.Usages = new HashSet<Usage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsageTypeID { get; set; }
        
        [Required]
        [StringLength(25)]
        public string UType { get; set; }

        public virtual ICollection<Usage> Usages { get; set; }
    }
}

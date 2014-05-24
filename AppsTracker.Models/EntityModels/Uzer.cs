using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Models.EntityModels
{
    public class Uzer
    {
        public Uzer()
        {
            //this.Applications = new HashSet<Aplication>();
            //this.AppsToBlocks = new HashSet<AppsToBlock>();
            //this.BlockedApps = new HashSet<BlockedApp>();
            //this.FileLogs = new HashSet<FileLog>();
            //this.Usages = new HashSet<Usage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(60)]
        public string Name { get; set; }

        public virtual ICollection<Aplication> Applications { get; set; }
        public virtual ICollection<AppsToBlock> AppsToBlocks { get; set; }
        public virtual ICollection<BlockedApp> BlockedApps { get; set; }
        public virtual ICollection<FileLog> FileLogs { get; set; }
        public virtual ICollection<Usage> Usages { get; set; }
    }
}

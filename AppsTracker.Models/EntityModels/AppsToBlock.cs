using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppsTracker.Models.Proxy;

namespace AppsTracker.Models.EntityModels
{
    public class AppsToBlock
    {
        public AppsToBlock() { }
        public AppsToBlock(Uzer uzer, Aplication aplication)
            : this()
        {
            this.UserID = uzer.UserID;
            this.ApplicationID = aplication.ApplicationID;
        }

        public void MapProxyValues(AppsToBlockProxy proxy)
        {
            this.Monday = proxy.Monday;
            this.Tuesday = proxy.Tuesday;
            this.Wednesday = proxy.Wednesday;
            this.Thursday = proxy.Thursday;
            this.Friday = proxy.Friday;
            this.Saturday = proxy.Saturday;
            this.Sunday = proxy.Sunday;
            this.TimeMax = proxy.TimeMax;
            this.TimeMin = proxy.TimeMin;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppsToBlockID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [Required]
        public bool Monday { get; set; }

        [Required]
        public bool Tuesday { get; set; }

        [Required]
        public bool Wednesday { get; set; }

        [Required]
        public bool Thursday { get; set; }

        [Required]
        public bool Friday { get; set; }

        [Required]
        public bool Saturday { get; set; }

        [Required]
        public bool Sunday { get; set; }

        [Required]
        public long TimeMin { get; set; }

        [Required]
        public long TimeMax { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }

        [ForeignKey("UserID")]
        public virtual Uzer User { get; set; }
    }
}

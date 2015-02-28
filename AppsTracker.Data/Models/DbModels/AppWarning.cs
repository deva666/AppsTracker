using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public enum AppWarningPeriod
    {
        Day,
        Week,
        Month
    }

    public class AppWarning
    {
        public AppWarning()
        {
            this.Applications = new HashSet<Aplication>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppWarningID { get; set; }

        [Required]
        public AppWarningPeriod Period { get; set; }

        [Required]
        public long Limit { get; set; }

        public ICollection<Aplication> Applications { get; set; }
    }
}

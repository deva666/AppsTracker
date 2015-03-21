using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public enum WarningSpan
    {
        Day,
        Week,
        Month
    }

    public enum TimeElapsedAction
    {
        Warn, 
        Shutdown,
        None
    }

    public class AppWarning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppWarningID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [Required]
        public WarningSpan WarningSpan { get; set; }

        [Required]
        public long Limit { get; set; }

        [Required]
        public TimeElapsedAction TimeElapsedAction { get; set; }
        
        [Required]
        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }
    }
}

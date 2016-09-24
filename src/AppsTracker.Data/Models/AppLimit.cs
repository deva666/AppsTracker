using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace AppsTracker.Data.Models
{
    public enum LimitSpan
    {
        Day,
        Week
    }

    public enum LimitReachedAction
    {
        Warn,
        Shutdown,
        WarnAndShutdown,
        None
    }


    public class AppLimit : IEntity
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AppLimitID")]
        public int ID { get; set; }

        [Required]
        public int ApplicationID { get; set; }


        [Required]
        public LimitSpan LimitSpan
        { get; set; }

        [Required]
        public long Limit { get; set; }

        [Required]
        public LimitReachedAction LimitReachedAction { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }

    }
}

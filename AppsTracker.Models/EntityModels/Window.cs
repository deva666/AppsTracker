using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppsTracker.Models.Utils;

namespace AppsTracker.Models.EntityModels
{
    public class Window
    {
        [NotMapped]
        public TimeSpan Duration
        {
            get
            {
                return GetWindowDuration();
            }
        }

        private TimeSpan GetWindowDuration()
        {
            long ticks = 0;
            foreach (var log in this.Logs)
            {
                ticks += log.Duration;
            }
            return new TimeSpan(ticks);
        }

        public Window()
        {
            this.Logs = new HashSet<Log>();
        }

        public Window(string title)
            : this()
        {
            this.Title = title.Truncate(1000);
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WindowID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [Required]
        [StringLength(1000)]
        public string Title { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }
        public virtual ICollection<Log> Logs { get; set; }

        public static Window CreateByTitleAndAppID(string title, int appID)
        {
            Window window = new Window();
            window.Title = title.Truncate(1000);
            window.ApplicationID = appID;
            return window;
        }
    }
}

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppsTracker.Common.Utils;

namespace AppsTracker.Data.Models
{
    public class Window : IEntity
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

        public Window(string title, int appId)
            : this(title)
        {
            this.ApplicationID = appId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("WindowID")]
        public int ID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        [Required]
        [StringLength(1000)]
        public string Title { get; set; }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }
        public virtual ICollection<Log> Logs { get; set; }

    }
}

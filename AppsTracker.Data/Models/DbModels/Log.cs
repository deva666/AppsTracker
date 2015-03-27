#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppsTracker.Data.Models
{
    public class Log : INotifyPropertyChanged
    {
        private bool finished = false;

        [NotMapped]
        public long Duration
        {
            get
            {
                return UtcDateEnded.Ticks - UtcDateCreated.Ticks;
            }
        }

        bool isSelected;
        [NotMapped]
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }


        public Log()
        {
            this.Screenshots = new HashSet<Screenshot>();
            this.DateCreated = this.DateEnded = DateTime.Now;
            this.UtcDateCreated = this.UtcDateEnded = DateTime.UtcNow;
        }

        public Log(int windowID)
            : this()
        {
            this.WindowID = windowID;
        }

        public Log(int windowID, int usageID)
            : this(windowID)
        {
            this.UsageID = usageID;
        }

        public Log(Window window, int usageID)
            : this()
        {
            this.Window = window;
            this.UsageID = usageID;
        }

        public void Finish()
        {
            if (!finished)
            {
                finished = true;
                DateEnded = DateTime.Now;
                UtcDateEnded = DateTime.UtcNow;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        [Required]
        public int WindowID { get; set; }

        [Required]
        public System.DateTime DateCreated { get; set; }

        [Required]
        public System.DateTime DateEnded { get; set; }

        [Required]
        public System.DateTime UtcDateCreated { get; set; }

        [Required]
        public System.DateTime UtcDateEnded { get; set; }

        [Required]
        public int UsageID { get; set; }

        public virtual ICollection<Screenshot> Screenshots { get; set; }

        [ForeignKey("WindowID")]
        public virtual Window Window { get; set; }

        [ForeignKey("UsageID")]
        public virtual Usage Usage { get; set; }
    }
}

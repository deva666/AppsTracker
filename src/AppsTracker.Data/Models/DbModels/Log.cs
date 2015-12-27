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
        [NotMapped]
        public long Duration
        {
            get
            {
                return Finished ? UtcDateEnded.Ticks - UtcDateCreated.Ticks : DateTime.UtcNow.Ticks - UtcDateCreated.Ticks;
            }
        }

        private bool isSelected;

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


        [NotMapped]
        public Guid LogInfoGuid { get; private set; }

        public Log()
        {
            this.Screenshots = new HashSet<Screenshot>();
            this.Finished = false;
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

        public Log(Window window, int usageID, Guid guid)
            : this()
        {
            this.Window = window;
            this.UsageID = usageID;
            this.LogInfoGuid = guid;
        }

        public Log(int windowId, int usageId, Guid guid)
            : this()
        {
            this.WindowID = windowId;
            this.UsageID = usageId;
            this.LogInfoGuid = guid;
        }

        public Log(Guid guid)
        {
            this.LogInfoGuid = guid;
        }

        public void Finish()
        {
            DateEnded = DateTime.Now;
            UtcDateEnded = DateTime.UtcNow;
            Finished = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        [Required]
        public int WindowID { get; set; }

        [Required]
        public bool Finished { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime DateEnded { get; set; }

        [Required]
        public DateTime UtcDateCreated { get; set; }

        [Required]
        public DateTime UtcDateEnded { get; set; }

        [Required]
        public int UsageID { get; set; }

        public virtual ICollection<Screenshot> Screenshots { get; set; }

        [ForeignKey("WindowID")]
        public virtual Window Window { get; set; }

        [ForeignKey("UsageID")]
        public virtual Usage Usage { get; set; }
    }
}

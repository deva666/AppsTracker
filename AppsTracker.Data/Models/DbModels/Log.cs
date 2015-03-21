#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AppsTracker.Data.Models
{
    public class Log : INotifyPropertyChanged
    {
        private bool _finished = false;
        [NotMapped]
        public bool Finished { get { return _finished; } }
        [NotMapped]
        public string AppName { get { return this.Window.Application.Name; } }
        [NotMapped]
        public string WindowTitle { get { return this.Window.Title; } }

        [NotMapped]
        public long Duration
        {
            get
            {
                return DateEnded.Ticks - DateCreated.Ticks;
            }
        }

        bool _isSelected;
        [NotMapped]
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        [NotMapped]
        public int UserID
        {
            get
            {
                return this.Window.Application.UserID;
            }
        }

        private StringBuilder stringBuilder;

        [NotMapped]
        public StringBuilder StringBuilder
        {
            get
            {
                if (stringBuilder == null)
                    stringBuilder = new StringBuilder();
                return stringBuilder;
            }
        }

        private StringBuilder stringBuilderRaw;

        [NotMapped]
        public StringBuilder StringBuilderRaw
        {
            get
            {
                if (stringBuilderRaw == null)
                    stringBuilderRaw = new StringBuilder();
                return stringBuilderRaw;
            }
        }

        public Log()
        {
            this.Screenshots = new HashSet<Screenshot>();
            this.DateCreated = this.DateEnded = DateTime.Now;
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
            if (!_finished)
            {
                _finished = true;
                DateEnded = DateTime.Now;
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
        public int UsageID { get; set; }

        [ForeignKey("WindowID")]
        public virtual Window Window { get; set; }

        public virtual ICollection<Screenshot> Screenshots { get; set; }

        [ForeignKey("UsageID")]
        public virtual Usage Usage { get; set; }
    }
}

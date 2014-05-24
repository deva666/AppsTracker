using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace AppsTracker.Models.EntityModels
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
            //   this.Screenshots = new HashSet<Screenshot>();
        }

        public Log(int windowID)
            : this()
        {
            this.WindowID = windowID;
            this.DateCreated = DateTime.Now;
        }

        public Log(int windowID, int usageID)
            : this(windowID)
        {
            this.UsageID = usageID;
        }

        public void Finish()
        {
            if (!_finished)
            {
                _finished = true;
                DateEnded = DateTime.Now;

                if (stringBuilder != null)
                    this.Keystrokes = stringBuilder.ToString();
                if (stringBuilderRaw != null)
                    this.KeystrokesRaw = stringBuilderRaw.ToString();
            }
        }

        public void AppendKeyLog(string str)
        {
            this.StringBuilder.Append(str);
        }

        public void AppendKeyLogRaw(string keyName)
        {
            this.StringBuilderRaw.Append(keyName);
        }

        public void RemoveLastKeyLogItem()
        {
            if (stringBuilder != null)
                if (stringBuilder.Length > 0)
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
        }

        public void AppendNewKeyLogLine()
        {
            this.StringBuilder.AppendLine();
            this.StringBuilderRaw.AppendLine();
        }

        public void AppendSpace()
        {
            this.StringBuilderRaw.Append(" ");
        }

        public void AppendTab()
        {
            this.StringBuilderRaw.Append("\t");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }
        [Required]
        public int WindowID { get; set; }
        [Required]
        public System.DateTime DateCreated { get; set; }
        [Column(TypeName = "ntext")]
        public string Keystrokes { get; set; }
        [Column(TypeName = "ntext")]
        public string KeystrokesRaw { get; set; }
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

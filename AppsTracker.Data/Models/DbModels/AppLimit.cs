using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using AppsTracker.Data.Utils;

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


    public class AppLimit : INotifyPropertyChanged
    {
        private ICommand _setLimitReachedCommand;

        [NotMapped]
        public ICommand SetLimitReachedCommand
        {
            get
            {
                return _setLimitReachedCommand ??
                    (_setLimitReachedCommand = new DelegateCommand(SetLimitReached));
            }
        }


        private ICommand _setLimitSpanCommand;

        [NotMapped]
        public ICommand SetLimitSpanCommand
        {
            get
            {
                return _setLimitSpanCommand ??
                    (_setLimitSpanCommand = new DelegateCommand(SetLimitSpan));
            }
        }


        private void SetLimitReached(object parameter)
        {
            this.LimitReachedAction = (LimitReachedAction)parameter;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("LimitReachedAction"));
        }


        private void SetLimitSpan(object parameter)
        {

        }


        [NotMapped]
        public bool HasChanges { get; private set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppLimitID { get; set; }

        [Required]
        public int ApplicationID { get; set; }

        private LimitSpan limitSpan;

        [Required]
        public LimitSpan LimitSpan
        {
            get { return limitSpan; }
            set
            {
                limitSpan = value;
                HasChanges = true;
            }
        }

        private long limit;

        [Required]
        public long Limit
        {
            get { return limit; }
            set
            {
                limit = value;
                HasChanges = true;
            }
        }

        private LimitReachedAction limitReachedAction;

        [Required]
        public LimitReachedAction LimitReachedAction
        {
            get { return limitReachedAction; }
            set
            {
                limitReachedAction = value;
                HasChanges = true;
            }
        }

        [ForeignKey("ApplicationID")]
        public virtual Aplication Application { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using AppsTracker.Data.Utils;
using AppsTracker.Utils;

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
        private bool isPopupLimitActionOpen;

        [NotMapped]
        public bool IsPopupLimitActionOpen
        {
            get { return isPopupLimitActionOpen; }
            set
            {
                isPopupLimitActionOpen = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsPopupLimitActionOpen"));
            }
        }


        private ICommand setLimitReachedCommand;

        [NotMapped]
        public ICommand SetLimitReachedCommand
        {
            get
            {
                return setLimitReachedCommand ??
                    (setLimitReachedCommand = new RelayCommand(SetLimitReached));
            }
        }
      

        private ICommand openLimitActionPopupCommand;

        [NotMapped]
        public ICommand OpenLimitActionPopupCommand
        {
            get
            {
                return openLimitActionPopupCommand ??
                (openLimitActionPopupCommand = new RelayCommand(OpenLimitActionPopup));
            }
        }

        private void SetLimitReached(object parameter)
        {
            var action = (string)parameter;
            if (action == "WARN")
                LimitReachedAction = Models.LimitReachedAction.Warn;
            else if (action == "SHUTDOWN")
                LimitReachedAction = Models.LimitReachedAction.Shutdown;
            else if (action == "BOTH")
                LimitReachedAction = Models.LimitReachedAction.WarnAndShutdown;

            IsPopupLimitActionOpen = false;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("LimitReachedAction"));
        }


        private void OpenLimitActionPopup()
        {
            IsPopupLimitActionOpen = !isPopupLimitActionOpen;
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

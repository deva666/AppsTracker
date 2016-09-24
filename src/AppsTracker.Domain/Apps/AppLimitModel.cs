using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Model;
using AppsTracker.Domain.Utils;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppLimitModel : ObservableObject
    {
        public AppLimitModel()
        { }

        public AppLimitModel(AppLimit appLimit)
        {
            ID = appLimit.ID;
            ApplicationID = appLimit.ApplicationID;
            Limit = appLimit.Limit;
            LimitSpan = appLimit.LimitSpan;
            LimitReachedAction = appLimit.LimitReachedAction;
        }

        private bool isPopupLimitActionOpen;

        public bool IsPopupLimitActionOpen
        {
            get { return isPopupLimitActionOpen; }
            set { SetPropertyValue(ref isPopupLimitActionOpen, value); }
        }

        public bool HasChanges { get; private set; }

        public int ID { get; set; }

        public int ApplicationID { get; set; }

        private LimitSpan limitSpan;

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

        public long Limit
        {
            get { return limit; }
            set
            {
                SetPropertyValue(ref limit, value);
                HasChanges = true;
            }
        }

        private LimitReachedAction limitReachedAction;

        public LimitReachedAction LimitReachedAction
        {
            get { return limitReachedAction; }
            set
            {
                SetPropertyValue(ref limitReachedAction, value);
                HasChanges = true;
            }
        }

        public Aplication Application { get; set; }

        private ICommand setLimitReachedCommand;

        public ICommand SetLimitReachedCommand
        {
            get
            {
                return setLimitReachedCommand ??
                    (setLimitReachedCommand = new RelayCommand(SetLimitReached));
            }
        }


        private ICommand openLimitActionPopupCommand;

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
                LimitReachedAction = Data.Models.LimitReachedAction.Warn;
            else if (action == "SHUTDOWN")
                LimitReachedAction = Data.Models.LimitReachedAction.Shutdown;
            else if (action == "BOTH")
                LimitReachedAction = Data.Models.LimitReachedAction.WarnAndShutdown;

            IsPopupLimitActionOpen = false;
        }


        private void OpenLimitActionPopup()
        {
            IsPopupLimitActionOpen = !isPopupLimitActionOpen;
        }
    }
}
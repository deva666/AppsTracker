using System.Windows.Input;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsLoggingViewModel : SettingsBaseViewModel
    {
        public override string Title
        {
            get { return "Logging"; }
        }

        private bool popupidleTimerIsOpen = false;

        public bool PopupIdleTimerIsOpen
        {
            get { return popupidleTimerIsOpen; }
            set { SetPropertyValue(ref popupidleTimerIsOpen, value); }
        }


        private bool popupOldLogsIsOpen = false;

        public bool PopupOldLogsIsOpen
        {
            get { return popupOldLogsIsOpen; }
            set { SetPropertyValue(ref popupOldLogsIsOpen, value); }
        }


        private ICommand changeIdleTimerCommand;

        public ICommand ChangeIdleTimerCommand
        {
            get { return changeIdleTimerCommand ?? (changeIdleTimerCommand = new DelegateCommand(ChangeIdleTimer)); }
        }


        private ICommand changeOldLogsDaysCommand;

        public ICommand ChangeOldLogsDaysCommand
        {
            get { return changeOldLogsDaysCommand ?? (changeOldLogsDaysCommand = new DelegateCommand(ChangeOldLogsDays)); }
        }


        private ICommand showPopUpCommand;

        public ICommand ShowPopupCommand
        {
            get { return showPopUpCommand ?? (showPopUpCommand = new DelegateCommand(ShowPopUp)); }
        }

        public SettingsLoggingViewModel()
            : base()
        {

        }

        private void ChangeIdleTimer(object parameter)
        {
            string interval = (string)parameter;
            int intOut;
            if (int.TryParse(interval, out intOut))
            {
                this.Settings.IdleTimer = intOut * 60 * 1000;
                SettingsChanging();
            }
            PopupIdleTimerIsOpen = false;
        }

        private void ChangeOldLogsDays(object parameter)
        {
            string daysString = parameter as string;
            if (daysString != null)
            {
                short days;
                short.TryParse(daysString, out days);
                this.Settings.OldLogDeleteDays = days;
                SettingsChanging();
            }
            PopupOldLogsIsOpen = false;
        }

        private void ShowPopUp(object popupSource)
        {
            string popup = (string)popupSource;

            if (popup == "OldLogs")
                PopupOldLogsIsOpen = !popupOldLogsIsOpen;

            else if (popup == "IdleTimer")
                PopupIdleTimerIsOpen = !popupidleTimerIsOpen;

        }
    }
}

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

        private bool _popupidleTimerIsOpen = false;
        public bool PopupIdleTimerIsOpen
        {
            get
            {
                return _popupidleTimerIsOpen;
            }
            set
            {
                _popupidleTimerIsOpen = value;
                PropertyChanging("PopupIdleTimerIsOpen");
            }
        }

        private bool _popupOldLogsIsOpen = false;
        public bool PopupOldLogsIsOpen
        {
            get
            {
                return _popupOldLogsIsOpen;
            }
            set
            {
                _popupOldLogsIsOpen = value;
                PropertyChanging("PopupOldLogsIsOpen");
            }
        }

        private ICommand _changeIdleTimerCommand;
        public ICommand ChangeIdleTimerCommand
        {
            get
            {
                return _changeIdleTimerCommand ?? (_changeIdleTimerCommand = new DelegateCommand(ChangeIdleTimer));
            }
        }

        private ICommand _changeOldLogsDaysCommand;
        public ICommand ChangeOldLogsDaysCommand
        {
            get
            {
                return _changeOldLogsDaysCommand ?? (_changeOldLogsDaysCommand = new DelegateCommand(ChangeOldLogsDays));
            }
        }

        private ICommand _showPopUpCommand;
        public ICommand ShowPopupCommand
        {
            get
            {
                return _showPopUpCommand ?? (_showPopUpCommand = new DelegateCommand(ShowPopUp));
            }
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
                PropertyChanging("Settings");
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
                PropertyChanging("Settings");
            }
            PopupOldLogsIsOpen = false;
        }

        private void ShowPopUp(object popupSource)
        {
            string popup = (string)popupSource;

            if (popup == "OldLogs")
                PopupOldLogsIsOpen = !_popupOldLogsIsOpen;

            else if (popup == "IdleTimer")
                PopupIdleTimerIsOpen = !_popupidleTimerIsOpen;

        }
    }
}

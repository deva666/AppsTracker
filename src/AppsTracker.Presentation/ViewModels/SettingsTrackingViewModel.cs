#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.MVVM;
using AppsTracker.Data.Repository;
using AppsTracker.Common.Communication;
using AppsTracker.Domain.Settings;

namespace AppsTracker.ViewModels
{
    [Export] 
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class SettingsTrackingViewModel : SettingsBaseViewModel
    {
        public override string Title
        {
            get { return "Tracking"; }
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

        [ImportingConstructor]
        public SettingsTrackingViewModel(IAppSettingsService settingsService, IMediator mediator)
            : base(settingsService, mediator)
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

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class MainViewModel : HostViewModel, ICommunicator
    {
        private readonly IDataService dataService;
        private readonly ISqlSettingsService settingsService;


        private bool isPopupCalendarOpen = false;

        public bool IsPopupCalendarOpen
        {
            get { return isPopupCalendarOpen; }
            set { SetPropertyValue(ref isPopupCalendarOpen, value); }
        }


        private bool isPopupUsersOpen = false;

        public bool IsPopupUsersOpen
        {
            get { return isPopupUsersOpen; }
            set { SetPropertyValue(ref isPopupUsersOpen, value); }
        }


        private bool isFilterApplied = false;

        public bool IsFilterApplied
        {
            get { return isFilterApplied; }
            set { SetPropertyValue(ref isFilterApplied, value); }
        }


        public override string Title
        {
            get { return "apps tracker"; }
        }


        private object toSettings;

        public object ToSettings
        {
            get { return toSettings; }
            set { SetPropertyValue(ref toSettings, value); }
        }


        public decimal DBSize
        {
            get { return Globals.GetDBSize(); }
        }


        public DateTime DateFrom
        {
            get { return Globals.DateFrom; }
            set
            {
                if (Globals.DateFrom == value)
                    return;
                IsFilterApplied = true;
                Globals.DateFrom = value;
                PropertyChanging("DateFrom");
                Mediator.NotifyColleagues(MediatorMessages.RefreshLogs);
            }
        }


        public DateTime DateTo
        {
            get { return Globals.DateTo; }
            set
            {
                if (Globals.DateTo == value)
                    return;
                IsFilterApplied = true;
                Globals.DateTo = value;
                PropertyChanging("DateTo");
                Mediator.NotifyColleagues(MediatorMessages.RefreshLogs);
            }
        }


        private string userName;

        public string UserName
        {
            get { return Globals.SelectedUserName; }
            set { SetPropertyValue(ref userName, value); }
        }


        public Setting UserSettings
        {
            get
            {
                return settingsService.Settings;
            }
        }


        public Uzer User
        {
            get
            {
                return Globals.SelectedUser;
            }
            set
            {
                if (value != null && value.UserID != Globals.SelectedUserID)
                {
                    Globals.ChangeUser(value);
                    PropertyChanging("User");
                    ClearFilter();
                }
            }
        }


        private IEnumerable<Uzer> userCollection;

        public IEnumerable<Uzer> UserCollection
        {
            get
            {
                if (userCollection == null)
                    GetUsers();
                return userCollection;
            }
        }


        private ICommand openPopupCommand;

        public ICommand OpenPopupCommand
        {
            get { return openPopupCommand ?? (openPopupCommand = new DelegateCommand(OpenPopup)); }
        }


        private ICommand getLogsByDateCommand;

        public ICommand GetLogsByDateCommand
        {
            get { return getLogsByDateCommand ?? (getLogsByDateCommand = new DelegateCommand(CloseDatesPopup)); }
        }


        private ICommand clearFilterCommand;

        public ICommand ClearFilterCommand
        {
            get { return clearFilterCommand ?? (clearFilterCommand = new DelegateCommand(ClearFilter)); }
        }


        private ICommand changeLoggingStatusCommand;

        public ICommand ChangeLoggingStatusCommand
        {
            get { return changeLoggingStatusCommand ?? (changeLoggingStatusCommand = new DelegateCommand(ChangeLoggingStatus)); }
        }


        private ICommand thisWeekCommand;

        public ICommand ThisWeekCommand
        {
            get { return thisWeekCommand ?? (thisWeekCommand = new DelegateCommand(ThisWeek)); }
        }


        private ICommand thisMonthCommand;

        public ICommand ThisMonthCommand
        {
            get { return thisMonthCommand ?? (thisMonthCommand = new DelegateCommand(ThisMonth)); }
        }


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public MainViewModel()
        {
            dataService = serviceResolver.Resolve<IDataService>();
            settingsService = serviceResolver.Resolve<ISqlSettingsService>();

            RegisterChild(() => new DataHostViewModel());
            RegisterChild(() => new StatisticsHostViewModel());
            RegisterChild(() => new SettingsHostViewModel());

            SelectedChild = GetChild(typeof(DataHostViewModel));
        }


        private void GetUsers()
        {
            userCollection = dataService.GetFiltered<Uzer>(u => u.UserID >= 0);
        }


        protected override void ChangePage(object parameter)
        {
            ToSettings = (SelectedChild == null || SelectedChild.GetType() == (Type)parameter) ? ToSettings : SelectedChild.GetType();
            base.ChangePage(parameter);
        }


        private void ChangeLoggingStatus()
        {
            var settings = UserSettings;
            settings.LoggingEnabled = !settings.LoggingEnabled;
            settingsService.SaveChanges(settings);
        }


        private void OpenPopup(object parameter)
        {
            string popup = parameter as string;
            if (popup == "Users")
            {
                if (IsPopupUsersOpen) IsPopupUsersOpen = false;
                else IsPopupUsersOpen = true;
            }
            else if (popup == "Calendar")
            {
                if (IsPopupCalendarOpen) IsPopupCalendarOpen = false;
                else IsPopupCalendarOpen = true;
            }
        }


        private void ClearFilter()
        {
            IsFilterApplied = false;
            Globals.ClearDateFilter();
            PropertyChanging("DateFrom");
            PropertyChanging("DateTo");
            Mediator.NotifyColleagues(MediatorMessages.RefreshLogs);
        }

        private void CloseDatesPopup()
        {
            if (IsPopupCalendarOpen)
                IsPopupCalendarOpen = false;
        }


        private void ThisMonth()
        {
            DateTime now = DateTime.Now;
            DateFrom = new DateTime(now.Year, now.Month, 1);
            int lastDay = DateTime.DaysInMonth(now.Year, now.Month);
            DateTo = new DateTime(now.Year, now.Month, lastDay);
        }


        private void ThisWeek()
        {
            DateTime now = DateTime.Today;
            int delta = DayOfWeek.Monday - now.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            DateFrom = now.AddDays(delta);
            DateTo = DateFrom.AddDays(6);
        }


        protected override void Disposing()
        {
            if (selectedChild != null)
            {
                selectedChild.Dispose();
                selectedChild = null;
            }
            base.Disposing();
        }
    }
}

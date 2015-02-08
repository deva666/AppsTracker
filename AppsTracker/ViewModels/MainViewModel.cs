#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class MainViewModel : HostViewModel, ICommunicator
    {
        #region Fields

        private IAppsService _appsService;
        private ISettingsService _settingsService;

        private bool _isPopupCalendarOpen = false;
        private bool _isPopupUsersOpen = false;
        private bool _isFilterApplied = false;

        private string _userName;
        private object _toSettings;

        private IEnumerable<Uzer> _uzerCollection;

        private ICommand _openPopupCommand;
        private ICommand _getLogsByDateCommand;
        private ICommand _clearFilterCommand;
        private ICommand _changeLoggingStatusCommand;
        private ICommand _thisWeekCommand;
        private ICommand _thisMonthCommand;

        #endregion

        #region Properties

        public bool IsPopupCalendarOpen
        {
            get
            {
                return _isPopupCalendarOpen;
            }
            set
            {
                _isPopupCalendarOpen = value;
                PropertyChanging("IsPopupCalendarOpen");
            }
        }

        public bool IsPopupUsersOpen
        {
            get
            {
                return _isPopupUsersOpen;
            }
            set
            {
                _isPopupUsersOpen = value;
                PropertyChanging("IsPopupUsersOpen");
            }
        }

        public bool IsFilterApplied
        {
            get
            {
                return _isFilterApplied;
            }
            set
            {
                _isFilterApplied = value;
                PropertyChanging("IsFilterApplied");
            }
        }

        public override string Title
        {
            get { return "apps tracker"; }
        }

        public object ToSettings
        {
            get
            {
                return _toSettings;
            }
            set
            {
                _toSettings = value;
                PropertyChanging("ToSettings");
            }
        }

        public decimal DBSize
        {
            get
            {
                return Globals.GetDBSize();
            }
        }

        public DateTime Date1
        {
            get
            {
                return Globals.Date1;
            }
            set
            {
                if (Globals.Date1 == value)
                    return;
                IsFilterApplied = true;
                Globals.Date1 = value;
                PropertyChanging("Date1");
                Mediator.NotifyColleagues(MediatorMessages.RefreshLogs);
            }
        }

        public DateTime Date2
        {
            get
            {
                return Globals.Date2;
            }
            set
            {
                if (Globals.Date2 == value)
                    return;
                IsFilterApplied = true;
                Globals.Date2 = value;
                PropertyChanging("Date2");
                Mediator.NotifyColleagues(MediatorMessages.RefreshLogs);
            }
        }

        public string UserName
        {
            get
            {
                return Globals.SelectedUserName;
            }
            set
            {
                _userName = value;
                PropertyChanging("UserName");
            }
        }

        public Setting UserSettings
        {
            get
            {
                return _settingsService.Settings;
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

        public IEnumerable<Uzer> UserCollection
        {
            get
            {
                if (_uzerCollection == null)
                    GetUsers();
                return _uzerCollection;
            }
        }

        public ICommand OpenPopupCommand
        {
            get
            {
                return _openPopupCommand ?? (_openPopupCommand = new DelegateCommand(OpenPopup));
            }
        }

        public ICommand GetLogsByDateCommand
        {
            get
            {
                return _getLogsByDateCommand ?? (_getLogsByDateCommand = new DelegateCommand(CloseDatesPopup));
            }
        }

        public ICommand ClearFilterCommand
        {
            get
            {
                return _clearFilterCommand ?? (_clearFilterCommand = new DelegateCommand(ClearFilter));
            }
        }

        public ICommand ChangeLoggingStatusCommand
        {
            get
            {
                return _changeLoggingStatusCommand ?? (_changeLoggingStatusCommand = new DelegateCommand(ChangeLoggingStatus));
            }
        }
        public ICommand ThisWeekCommand
        {
            get
            {
                return _thisWeekCommand ?? (_thisWeekCommand = new DelegateCommand(ThisWeek));
            }
        }
        public ICommand ThisMonthCommand
        {
            get
            {
                return _thisMonthCommand ?? (_thisMonthCommand = new DelegateCommand(ThisMonth));
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            _appsService = ServiceFactory.Get<IAppsService>();
            _settingsService = ServiceFactory.Get<ISettingsService>();

            Register<DataHostViewModel>(() => new DataHostViewModel());
            Register<StatisticsHostViewModel>(() => new StatisticsHostViewModel());
            Register<SettingsHostViewModel>(() => new SettingsHostViewModel());

            SelectedChild = Resolve(typeof(DataHostViewModel));
        }

        #endregion

        private void GetUsers()
        {
            _uzerCollection = _appsService.GetFiltered<Uzer>(u => u.UserID >= 0);
        }

        #region Command Methods

        protected override void ChangePage(object parameter)
        {
            ToSettings = (SelectedChild == null || SelectedChild.GetType() == (Type)parameter) ? ToSettings : SelectedChild.GetType();
            base.ChangePage(parameter);
        }

        private void ChangeLoggingStatus()
        {
            var settings = UserSettings;
            settings.LoggingEnabled = !settings.LoggingEnabled;
            _settingsService.SaveChanges(settings);
            PropertyChanging("UserSettings");
        }

        private void OpenPopup(object parameter)
        {
            string popup = parameter as string;
            if (popup == "Users")
            {
                if (IsPopupUsersOpen) IsPopupUsersOpen = false;
                else IsPopupUsersOpen = true;
            }
            if (popup == "Calendar")
            {
                if (IsPopupCalendarOpen) IsPopupCalendarOpen = false;
                else IsPopupCalendarOpen = true;
            }
        }

        private void ClearFilter()
        {
            IsFilterApplied = false;
            Globals.ClearDateFilter();
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
            Date1 = new DateTime(now.Year, now.Month, 1);
            int lastDay = DateTime.DaysInMonth(now.Year, now.Month);
            Date2 = new DateTime(now.Year, now.Month, lastDay);
        }
        private void ThisWeek()
        {
            DateTime now = DateTime.Today;
            int delta = DayOfWeek.Monday - now.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            Date1 = now.AddDays(delta);
            Date2 = Date1.AddDays(6);
        }

        #endregion

        protected override void Disposing()
        {
            if (_selectedChild != null)
            {
                _selectedChild.Dispose();
                _selectedChild = null;
            }
            base.Disposing();
        }
    }
}

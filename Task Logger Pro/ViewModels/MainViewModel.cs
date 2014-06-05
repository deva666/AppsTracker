using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Task_Logger_Pro.Logging;
using Task_Logger_Pro.Pages.ViewModels;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro.ViewModels
{
    class MainViewModel : HostViewModel, ICommunicator
    {
        #region Fields

        bool _isPopupCalendarOpen = false;
        bool _isPopupUsersOpen = false;
        bool _isFilterApplied = false;

        string _userName;
        string _toSettings;

        IEnumerable<Uzer> _uzerCollection;

        ICommand _openPopupCommand;
        ICommand _getLogsByDateCommand;
        ICommand _clearFilterCommand;
        ICommand _changeLoggingStatusCommand;
        ICommand _thisWeekCommand;
        ICommand _thisMonthCommand;

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

        public string ToSettings
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
                Mediator.NotifyColleagues<object>(MediatorMessages.RefreshLogs);
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
                Mediator.NotifyColleagues<object>(MediatorMessages.RefreshLogs);
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

        public SettingsProxy UserSettings
        {
            get
            {
                return App.UzerSetting;
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
                    Mediator.NotifyColleagues<object>(MediatorMessages.RefreshLogs);
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
                if (_openPopupCommand == null)
                    _openPopupCommand = new DelegateCommand(OpenPopup);
                return _openPopupCommand;
            }
        }

        public ICommand GetLogsByDateCommand
        {
            get
            {
                if (_getLogsByDateCommand == null)
                    _getLogsByDateCommand = new DelegateCommand(CloseDatesPopup);
                return _getLogsByDateCommand;
            }
        }

        public ICommand ClearFilterCommand
        {
            get
            {
                if (_clearFilterCommand == null)
                    _clearFilterCommand = new DelegateCommand(ClearFilter);
                return _clearFilterCommand;
            }
        }

        public ICommand ChangeLoggingStatusCommand
        {
            get
            {
                if (_changeLoggingStatusCommand == null)
                    _changeLoggingStatusCommand = new DelegateCommand(ChangeLoggingStatus);
                return _changeLoggingStatusCommand;
            }
        }
        public ICommand ThisWeekCommand
        {
            get
            {
                return _thisWeekCommand == null ? _thisWeekCommand = new DelegateCommand(ThisWeek) : _thisWeekCommand;
            }
        }
        public ICommand ThisMonthCommand
        {
            get
            {
                return _thisMonthCommand == null ? _thisMonthCommand = new DelegateCommand(ThisMonth) : _thisMonthCommand;
            }
        }

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            this.SelectedChild = new DataHostViewModel();

            //DataHostViewModel dataHostViewModel = new DataHostViewModel();
            //StatisticsHostViewModel statisticsHostViewModel = new StatisticsHostViewModel();
            //SettingsHostViewModel settingsHostViewModel = new SettingsHostViewModel();

            //var tempChildren = new List<IChildVM>() { dataHostViewModel, statisticsHostViewModel, settingsHostViewModel };

            //this.Children = new ReadOnlyCollection<IChildVM>(tempChildren);
        }

        #endregion

        private void GetUsers()
        {
            using (var context = new AppsEntities())
            {
                _uzerCollection = (from u in context.Users
                                   select u).ToList();
            }
        }

        #region Command Methods

        private void ChangeLoggingStatus()
        {
            if (UserSettings.LoggingEnabled)
            {
                UserSettings.LoggingEnabled = false;
            }
            else
            {
                UserSettings.LoggingEnabled = true;
            }
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
            Mediator.NotifyColleagues<object>(MediatorMessages.RefreshLogs);
        }

        private void CloseDatesPopup()
        {
            if (IsPopupCalendarOpen)
                IsPopupCalendarOpen = false;
        }

        protected override void ChangePage(object parameter)
        {
            string viewName = parameter as string;
            if (viewName == null)
                return;
            if (viewName.ToLower() == "settings")
                ToSettings = this.SelectedChild.Title;

            switch (viewName)
            {
                case "data":
                    this.SelectedChild = new DataHostViewModel();
                    break;
                case "statistics":
                    this.SelectedChild = new StatisticsHostViewModel();
                    break;
                case "settings":
                    this.SelectedChild = new SettingsHostViewModel();
                    break;
                default:
                    break;
            }

            PropertyChanging("DBSize");
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
            //foreach (var child in this.Children)
            //{
            //    ((ViewModelBase)child).Dispose();
            //}

            this._selectedChild = null;
            //this.Children = null;
            base.Disposing();
        }

    }
}

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class MainViewModel : HostViewModel
    {
        private readonly IDataService dataService;
        private readonly ISqlSettingsService settingsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

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


        private Type toSettings;

        public Type ToSettings
        {
            get { return toSettings; }
            set { SetPropertyValue(ref toSettings, value); }
        }


        public decimal DBSize
        {
            get { return dataService.GetDBSize(); }
        }


        public DateTime DateFrom
        {
            get { return trackingService.DateFrom; }
            set
            {
                if (trackingService.DateFrom == value)
                    return;
                IsFilterApplied = true;
                trackingService.DateFrom = value;
                PropertyChanging("DateFrom");
                mediator.NotifyColleagues(MediatorMessages.REFRESH_LOGS);
            }
        }


        public DateTime DateTo
        {
            get { return trackingService.DateTo; }
            set
            {
                if (trackingService.DateTo == value)
                    return;
                IsFilterApplied = true;
                trackingService.DateTo = value;
                PropertyChanging("DateTo");
                mediator.NotifyColleagues(MediatorMessages.REFRESH_LOGS);
            }
        }


        private string userName;

        public string UserName
        {
            get { return trackingService.SelectedUserName; }
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
                return trackingService.SelectedUser;
            }
            set
            {
                if (value != null && value.UserID != trackingService.SelectedUserID)
                {
                    trackingService.ChangeUser(value);
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

        private ICommand goToDataCommand;

        public ICommand GoToDataCommand
        {
            get { return goToDataCommand ?? (goToDataCommand = new DelegateCommand(GoToData)); }
        }


        private ICommand goToStatsCommand;

        public ICommand GoToStatsCommand
        {
            get { return goToStatsCommand ?? (goToStatsCommand = new DelegateCommand(GoToStats)); }
        }

        private ICommand goToSettingsCommand;

        public ICommand GoToSettingsCommand
        {
            get { return goToSettingsCommand ?? (goToSettingsCommand = new DelegateCommand(GoToSettings)); }
        }


        private ICommand returnFromSettingsCommand;

        public ICommand ReturnFromSettingsCommand
        {
            get { return returnFromSettingsCommand ?? (returnFromSettingsCommand = new DelegateCommand(ReturnFromSettings)); }
        }



        [ImportingConstructor]
        public MainViewModel(IDataService dataService,
                             ISqlSettingsService settingsService,
                             ITrackingService trackingService,
                             IMediator mediator,
                             ExportFactory<DataHostViewModel> dataVMFactory,
                             ExportFactory<StatisticsHostViewModel> statisticsVMFactory,
                             ExportFactory<SettingsHostViewModel> settingsVMFactory)
        {
            this.dataService = dataService;
            this.settingsService = settingsService;
            this.trackingService = trackingService;
            this.mediator = mediator;         

            RegisterChild(() => ProduceValue(dataVMFactory));
            RegisterChild(() => ProduceValue(statisticsVMFactory));
            RegisterChild(() => ProduceValue(settingsVMFactory));

            SelectedChild = GetChild<DataHostViewModel>();
        }


        private void GetUsers()
        {
            userCollection = dataService.GetFiltered<Uzer>(u => u.UserID > 0);
        }


        private void ChangeLoggingStatus()
        {
            var settings = UserSettings;
            settings.LoggingEnabled = !settings.LoggingEnabled;
            settingsService.SaveChanges(settings);
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
            else if (popup == "Calendar")
            {
                if (IsPopupCalendarOpen) IsPopupCalendarOpen = false;
                else IsPopupCalendarOpen = true;
            }
        }


        private void ClearFilter()
        {
            IsFilterApplied = false;
            trackingService.ClearDateFilter();
            PropertyChanging("DateFrom");
            PropertyChanging("DateTo");
            mediator.NotifyColleagues(MediatorMessages.REFRESH_LOGS);
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


        private void GoToData()
        {
            SelectedChild = GetChild<DataHostViewModel>();
        }


        private void GoToStats()
        {
            SelectedChild = GetChild<StatisticsHostViewModel>();
        }


        private void GoToSettings()
        {
            if (ToSettings != SelectedChild.GetType())
                ToSettings = SelectedChild.GetType();
            SelectedChild = GetChild<SettingsHostViewModel>();
        }


        private void ReturnFromSettings()
        {
            if (toSettings == null)
                throw new InvalidOperationException("to settings should be assigned a value");

            SelectedChild = GetChild(toSettings);
            toSettings = null;
        }
    }
}

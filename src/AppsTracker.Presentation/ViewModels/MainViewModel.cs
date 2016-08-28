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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Repository;
using AppsTracker.Common.Communication;
using AppsTracker.Service.Web;
using AppsTracker.Tracking;
using AppsTracker.Domain.Settings;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class MainViewModel : HostViewModel
    {
        private readonly IRepository repository;
        private readonly IAppSettingsService settingsService;
        private readonly IUserSettingsService userSettingsService;
        private readonly ITrackingService trackingService;
        private readonly IReleaseNotesService releaseNotesService;
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


        private bool multipleUsers;

        public bool MultipleUsers
        {
            get { return multipleUsers; }
        }


        private bool newVersionAvailable;

        public bool NewVersionAvailable
        {
            get { return newVersionAvailable; }
            set { SetPropertyValue(ref newVersionAvailable, value); }
        }


        public bool DisableNotifyForNewVersion
        {
            get { return userSettingsService.AppSettings.DisableNotifyForNewVersion; }
            set { userSettingsService.AppSettings.DisableNotifyForNewVersion = value; }
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
            get { return repository.GetDBSize(); }
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


        private ICommand closeNewVersionNotifierCommand;

        public ICommand CloseNewVersionNotifierCommand
        {
            get { return closeNewVersionNotifierCommand ?? (closeNewVersionNotifierCommand = new DelegateCommand(CloseNewVersionNotifier)); }
        }


        private ICommand showWebCommand;

        public ICommand ShowWebCommand
        {
            get { return showWebCommand ?? (showWebCommand = new DelegateCommand(ShowWeb)); }
        }


        [ImportingConstructor]
        public MainViewModel(IRepository repository,
                             IAppSettingsService settingsService,
                             IUserSettingsService userSettingsService,
                             ITrackingService trackingService,
                             IReleaseNotesService releaseNotesService,
                             IMediator mediator,
                             ExportFactory<DataHostViewModel> dataVMFactory,
                             ExportFactory<StatisticsHostViewModel> statisticsVMFactory,
                             ExportFactory<SettingsHostViewModel> settingsVMFactory)
        {
            this.repository = repository;
            this.settingsService = settingsService;
            this.userSettingsService = userSettingsService;
            this.trackingService = trackingService;
            this.releaseNotesService = releaseNotesService;
            this.mediator = mediator;

            RegisterChild(() => ProduceViewModel(dataVMFactory));
            RegisterChild(() => ProduceViewModel(statisticsVMFactory));
            RegisterChild(() => ProduceViewModel(settingsVMFactory));

            SelectedChild = GetChild<DataHostViewModel>();

            multipleUsers = repository.GetFiltered<Uzer>(u => u.UserID > 0).Count() > 1;

            if (!userSettingsService.AppSettings.DisableNotifyForNewVersion)
            {
                releaseNotesService.GetReleaseNotesAsync()
                    .ContinueWith(OnGetReleaseNotes, TaskContinuationOptions.NotOnFaulted);
            }
        }


        private void OnGetReleaseNotes(Task<IEnumerable<ReleaseNote>> preceedingTask)
        {
            var notes = preceedingTask.Result;
            var versions = new List<Version>(notes.Count());
            foreach (var note in notes)
            {
                Version version;
                if (Version.TryParse(note.Version, out version))
                    versions.Add(version);
            }
            var latestVersion = versions.OrderByDescending(v => v).FirstOrDefault();
            if (latestVersion == null)
            {
                return;
            }
            var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (latestVersion > currentVersion)
            {
                NewVersionAvailable = true;
            }

        }

        private void GetUsers()
        {
            userCollection = repository.GetFiltered<Uzer>(u => u.UserID > 0);
        }


        private void ChangeLoggingStatus()
        {
            var settings = UserSettings;
            settings.TrackingEnabled = !settings.TrackingEnabled;
            settingsService.SaveChanges(settings);
            mediator.NotifyColleagues(MediatorMessages.TRACKING_ENABLED_CHANGING, settings.TrackingEnabled);
            mediator.NotifyColleagues(settings.TrackingEnabled ? MediatorMessages.RESUME_TRACKING : MediatorMessages.STOP_TRACKING);
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
            if (toSettings != SelectedChild.GetType())
            {
                toSettings = SelectedChild.GetType();
            }
            SelectedChild = GetChild<SettingsHostViewModel>();
        }


        private void ReturnFromSettings()
        {
            if (toSettings == null)
            {
                SelectedChild = GetChild<DataHostViewModel>();
            }
            SelectedChild = GetChild(toSettings);
        }

        private void CloseNewVersionNotifier()
        {
            NewVersionAvailable = false;
        }

        private void ShowWeb()
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.theappstracker.com");
            }
            catch
            {
            }
        }
    }
}

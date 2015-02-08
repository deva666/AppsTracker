#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AppsTracker.DAL.Service;
using AppsTracker.Models.ChartModels;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;

namespace AppsTracker.Pages.ViewModels
{
    internal class Data_logsViewModel : ViewModelBase, ICommunicator
    {
        #region Fields

        IAppsService _appsService;
        IChartService _chartService;

        bool _chartVisible;

        string _overallAppDuration;
        string _overallWindowDuration;

        DateTime _date1;
        DateTime _date2;

        AsyncProperty<IEnumerable<Aplication>> _aplicationList;
        AsyncProperty<IEnumerable<TopAppsModel>> _topAppsList;
        AsyncProperty<IEnumerable<TopWindowsModel>> _topWindowsList;
        AsyncProperty<IEnumerable<DailyWindowSeries>> _chartList;

        Aplication _selectedApplication;

        TopAppsModel _topAppsOverall;

        List<MenuItem> _allUsersList;

        ICommand _sortViewCommand;
        ICommand _addProcessToBlockedListCommand;
        ICommand _overallAppSelectionChangedCommand;
        ICommand _overallWindowSelectionChangedCommand;
        ICommand _addDaysCommand1;
        ICommand _addDaysCommand2;
        ICommand _changeDaysCommand;

        #endregion

        #region Properties

        public bool ChartVisible
        {
            get
            {
                return _chartVisible;
            }
            set
            {
                _chartVisible = value;
                PropertyChanging("ChartVisible");
            }
        }
        public override string Title
        {
            get
            {
                return "APPS";
            }
        }
        public string OverallAppDuration
        {
            get
            {
                return _overallAppDuration;
            }
            set
            {
                _overallAppDuration = value;
                PropertyChanging("OverallAppDuration");
            }
        }
        public string OverallWindowDuration
        {
            get
            {
                return _overallWindowDuration;
            }
            set
            {
                _overallWindowDuration = value;
                PropertyChanging("OverallWindowDuration");
            }
        }

        public DateTime Date1
        {
            get
            {
                return _date1;
            }
            set
            {
                _date1 = value;
                PropertyChanging("Date1");
                if (SelectedApplication != null)
                    _topAppsList.Reload();

            }
        }
        public DateTime Date2
        {
            get
            {
                return _date2;
            }
            set
            {
                _date2 = value;
                PropertyChanging("Date2");
                if (SelectedApplication != null)
                    _topAppsList.Reload();
            }
        }

        public AsyncProperty<IEnumerable<Aplication>> AplicationList
        {
            get
            {
                return _aplicationList;
            }
        }
        public AsyncProperty<IEnumerable<TopAppsModel>> TopAppsList
        {
            get
            {
                return _topAppsList;
            }
        }
        public AsyncProperty<IEnumerable<TopWindowsModel>> TopWindowsList
        {
            get
            {
                return _topWindowsList;
            }
        }
        public AsyncProperty<IEnumerable<DailyWindowSeries>> ChartList
        {
            get
            {
                return _chartList;
            }
        }

        public TopAppsModel TopAppsOverall
        {
            get
            {
                return _topAppsOverall;
            }
            set
            {
                _topAppsOverall = value;
                PropertyChanging("TopAppsOverall");
                OverallWindowDuration = string.Empty;
            }
        }
        public Aplication SelectedApplication
        {
            get
            {
                return _selectedApplication;
            }
            set
            {
                _selectedApplication = value;
                PropertyChanging("SelectedApplication");
                ChartVisible = false;
                if (value != null)
                {
                    _topAppsList.Reload();
                }
            }
        }

        public List<MenuItem> AllUsersList
        {
            get
            {
                GetAllUsers();
                return _allUsersList;
            }
        }

        public ICommand SortViewCommand
        {
            get
            {
                return _sortViewCommand == null ? _sortViewCommand = new DelegateCommand(SortViewProcesses) : _sortViewCommand;
            }
        }
        public ICommand AddProcessToBlockedListCommand
        {
            get
            {
                return _addProcessToBlockedListCommand == null ? _addProcessToBlockedListCommand = new DelegateCommand(AddAplicationToBlockedList) : _addProcessToBlockedListCommand;
            }
        }

        public ICommand OverallAppSelectionChangedCommand
        {
            get
            {
                return _overallAppSelectionChangedCommand == null ? _overallAppSelectionChangedCommand = new DelegateCommand(OverallAppSelectionChanged) : _overallAppSelectionChangedCommand;
            }
        }
        public ICommand OverallWindowSelectionChangedCommand
        {
            get
            {
                return _overallWindowSelectionChangedCommand == null ? _overallWindowSelectionChangedCommand = new DelegateCommand(OverallWindowSelectionChanged) : _overallWindowSelectionChangedCommand;
            }
        }
        public ICommand AddDaysCommand1
        {
            get
            {
                return _addDaysCommand1 == null ? _addDaysCommand1 = new DelegateCommand(AddDays1) : _addDaysCommand1;
            }
        }
        public ICommand AddDaysCommand2
        {
            get
            {
                return _addDaysCommand2 == null ? _addDaysCommand2 = new DelegateCommand(AddDays2) : _addDaysCommand2;
            }
        }
        public ICommand ChangeDaysCommand
        {
            get
            {
                return _changeDaysCommand == null ? _changeDaysCommand = new DelegateCommand(ChangeDays) : _changeDaysCommand;
            }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        public Data_logsViewModel()
        {
            _appsService = ServiceFactory.Get<IAppsService>();
            _chartService = ServiceFactory.Get<IChartService>();

            _aplicationList = new AsyncProperty<IEnumerable<Aplication>>(GetContent, this);
            _topAppsList = new AsyncProperty<IEnumerable<TopAppsModel>>(GetTopApps, this);
            _topWindowsList = new AsyncProperty<IEnumerable<TopWindowsModel>>(GetTopWindows, this);
            _chartList = new AsyncProperty<IEnumerable<DailyWindowSeries>>(GetChartContent, this);


            Mediator.Register(MediatorMessages.ApplicationAdded, new Action<Aplication>(ApplicationAdded));
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(_aplicationList.Reload));
        }

        #region Loader Methods

        private void LoadAppsOverall()
        {
            _topAppsList.Reload();
            ChartVisible = false;
        }

        private IEnumerable<Aplication> GetContent()
        {
            return _appsService.GetFiltered<Aplication>(a => a.User.UserID == Globals.SelectedUserID
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= Globals.Date1).Any()
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= Globals.Date2).Any())
                                                           .ToList()
                                                           .Distinct();
        }

        private IEnumerable<TopAppsModel> GetTopApps()
        {
            var app = SelectedApplication;
            if (app == null)
                return null;

            return _chartService.GetLogTopApps(Globals.SelectedUserID, app.ApplicationID, app.Name, Globals.Date1, Globals.Date2);
        }

        private IEnumerable<TopWindowsModel> GetTopWindows()
        {
            var topApps = TopAppsOverall;
            if (TopAppsList.Result == null || topApps == null)
                return null;

            var days = TopAppsList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);
            return _chartService.GetLogTopWindows(Globals.SelectedUserID, topApps.AppName, days);
        }

        private IEnumerable<DailyWindowSeries> GetChartContent()
        {
            var topApps = TopAppsOverall;

            if (TopAppsList.Result == null || topApps == null || TopWindowsList.Result == null)
                return null;

            var selectedWindows = TopWindowsList.Result.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            var days = TopAppsList.Result.Where(t => t.IsSelected).Select(t => t.DateTime);

            return _chartService.GetDailyWindowSeries(Globals.SelectedUserID, topApps.AppName, selectedWindows, days);
        }

        #endregion

        #region Mediator Methods

        private void ApplicationAdded(Aplication app)
        {
            _aplicationList.Reload();

            _chartList.Reset();
            _topAppsList.Reset();
            _topWindowsList.Reset();
        }

        #endregion

        #region Command Methods

        private void SortViewProcesses(object parameter)
        {
            if (AplicationList == null)
                return;
            string propertyName = parameter as string;
            ICollectionView view;
            switch (propertyName)
            {
                case "Name":
                    view = CollectionViewSource.GetDefaultView(AplicationList);
                    break;
                default:
                    view = null;
                    break;
            }

            if (view != null)
            {
                view.SortDescriptions.Clear();

                if (view.SortDescriptions.Count > 0 && view.SortDescriptions[0].PropertyName == propertyName && view.SortDescriptions[0].Direction == ListSortDirection.Ascending)
                    view.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Descending));
                else
                    view.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));

            }
        }

        private void AddAplicationToBlockedList(object parameter)
        {
            Dictionary<string, ObservableCollection<object>> processesDict = parameter as Dictionary<string, ObservableCollection<object>>;
            if (processesDict != null)
            {
                string username = processesDict.Keys.ElementAtOrDefault(0);
                if (processesDict.ContainsKey(username))
                {
                    ObservableCollection<object> objectCollection = processesDict[username];
                    List<Aplication> appList = objectCollection.Select(o => o as Aplication).ToList();
                    if (appList != null)
                    {
                        var notifyList = _appsService.AddToBlockedList(appList, username, Globals.UserID);
                        Mediator.NotifyColleagues<IList<AppsToBlock>>(MediatorMessages.AppsToBlockChanged, notifyList);
                    }
                }
            }

        }

        private void OverallAppSelectionChanged()
        {
            ChartVisible = false;

            if (TopAppsList.Result == null)
                return;

            long ticks = 0;
            var filteredApps = TopAppsList.Result.Where(t => t.IsSelected);

            if (filteredApps.Count() == 0)
            {
                _topWindowsList.Reset();
                return;
            }

            foreach (var app in filteredApps)
                ticks += app.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            OverallAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            _topWindowsList.Reload();
        }
        private void OverallWindowSelectionChanged()
        {
            if (_topWindowsList.Result == null)
            {
                ChartList.Reset();
                ChartVisible = false;
                return;
            }

            if (_topWindowsList.Result.Where(w => w.IsSelected).Count() == 0)
            {
                ChartVisible = false;
                return;
            }

            ChartVisible = true;
            long ticks = 0;

            foreach (var window in _topWindowsList.Result.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            OverallWindowDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            _chartList.Reload();
        }

        private void GetAllUsers()
        {
            if (_allUsersList == null)
                _allUsersList = new List<MenuItem>();

            MenuItem menuItem = new MenuItem();
            menuItem.Header = "All users";
            _allUsersList.Add(menuItem);
            var users = _appsService.GetFiltered<Uzer>(u => u.Name != null);

            foreach (var user in users)
            {
                menuItem = new MenuItem();
                menuItem.Header = user.Name;
                _allUsersList.Add(menuItem);
            }
        }

        private void AddDays1(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "+":
                    Date1 = Date1.AddDays(1d);
                    break;
                case "-":
                    Date1 = Date1.AddDays(-1d);
                    break;
                default:
                    Date1 = DateTime.Today;
                    break;
            }
        }

        private void AddDays2(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "+":
                    Date2 = Date2.AddDays(1d);
                    break;
                case "-":
                    Date2 = Date2.AddDays(-1d);
                    break;
                default:
                    Date2 = DateTime.Today;
                    break;
            }
        }

        private void ChangeDays(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "Today":
                    _date1 = DateTime.Now.Date;
                    _date2 = Date1.AddHours(23.99d);
                    PropertyChanging("Date1");
                    PropertyChanging("Date2");
                    _topAppsList.Reload();
                    break;
                case "This week":
                    {
                        DateTime now = DateTime.Today;
                        int delta = DayOfWeek.Monday - now.DayOfWeek;
                        if (delta > 0)
                            delta -= 7;
                        _date1 = now.AddDays(delta);
                        _date2 = Date1.AddDays(6);
                        PropertyChanging("Date1");
                        PropertyChanging("Date2");
                        LoadAppsOverall();
                    }
                    break;
                case "This month":
                    {
                        DateTime now = DateTime.Now;
                        _date1 = new DateTime(now.Year, now.Month, 1);
                        int lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                        _date2 = new DateTime(now.Year, now.Month, lastDay);
                        PropertyChanging("Date1");
                        PropertyChanging("Date2");
                        LoadAppsOverall();
                    }
                    break;
                default:
                    break;
            }
        }

        #endregion

    }
}

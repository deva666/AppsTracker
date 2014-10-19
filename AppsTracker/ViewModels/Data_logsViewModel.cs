using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Logging;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Management;
using System.Windows.Controls;
using Task_Logger_Pro;
using System.Diagnostics;
using System.Threading.Tasks;
using Task_Logger_Pro.Controls;
using System.Data.Entity;
using Task_Logger_Pro.Utils;
using Task_Logger_Pro.ViewModels;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;
using AppsTracker.DAL.Repos;

namespace Task_Logger_Pro.Pages.ViewModels
{
    class Data_logsViewModel : ViewModelBase, IWorker, IChildVM, ICommunicator
    {
        #region Fields

        bool _working;
        bool _chartVisible;

        string _overallAppDuration;
        string _overallWindowDuration;

        DateTime _date1;
        DateTime _date2;

        IEnumerable<Aplication> _aplicationList;
        IEnumerable<TopAppsModel> _topAppsList;
        IEnumerable<TopWindowsModel> _topWindowsList;
        IEnumerable<DailyWindowSeries> _chartList;
        IEnumerable<Log> _cachedLogs;

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
        public string Title
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
                {
                    LoadAppsOverall();
                }
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
                {
                    LoadAppsOverall();
                }
            }
        }
        public bool IsContentLoaded
        {
            get;
            private set;
        }
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                PropertyChanging("Working");
            }
        }

        public IEnumerable<Aplication> AplicationList
        {
            get
            {
                return _aplicationList;
            }
            set
            {
                _aplicationList = value;
                PropertyChanging("AplicationList");
            }
        }
        public IEnumerable<TopAppsModel> TopAppsList
        {
            get
            {
                return _topAppsList;
            }
            set
            {
                _topAppsList = value;
                PropertyChanging("TopAppsList");
            }
        }
        public IEnumerable<TopWindowsModel> TopWindowsList
        {
            get
            {
                return _topWindowsList;
            }
            set
            {
                _topWindowsList = value;
                PropertyChanging("TopWindowsList");
            }
        }
        public IEnumerable<DailyWindowSeries> ChartList
        {
            get
            {
                return _chartList;
            }
            set
            {
                _chartList = value;
                PropertyChanging("ChartList");
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
                _cachedLogs = null;
                ChartVisible = false;
                if (value != null)
                    LoadAppsOverall();
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

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion

        #region Constructor

        public Data_logsViewModel()
        {
            Mediator.Register(MediatorMessages.ApplicationAdded, new Action<Aplication>(ApplicationAdded));
            Mediator.Register(MediatorMessages.RefreshLogs, new Action(LoadContent));
        }

        #endregion

        #region Loader Methods

        public async void LoadContent()
        {
            Working = true;
            OverallAppDuration = string.Empty;
            OverallWindowDuration = string.Empty;
            Date1 = Globals.Date1;
            Date2 = Globals.Date2;
            AplicationList = await GetContentFromRepo();
            Working = false;
            IsContentLoaded = true;
        }

        private async void LoadAppsOverall()
        {
            Working = true;
            TopAppsList = await GetTopAppsAsync();
            Working = false;
            ChartVisible = false;
        }

        private async void LoadWindowsOverall()
        {
            Working = true;
            TopWindowsList = await GetTopWindowsAsync();
            Working = false;
        }

        private async void LoadChart()
        {
            Working = true;
            ChartList = await GetChartContentAsync();
            Working = false;
        }

        private Task<IEnumerable<Aplication>> GetContentFromRepo()
        {
            return AplicationRepo.Instance.GetFilteredAsync(a => a.User.UserID == Globals.SelectedUserID
                                                           && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= Globals.Date1).Any()
                                                           && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= Globals.Date2).Any());
        }

        private async Task<IEnumerable<TopAppsModel>> GetTopAppsAsync()
        {
            Aplication app = SelectedApplication;
            if (app == null)
                return null;

            var logs = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                         && l.DateCreated >= Globals.Date1
                                                                         && l.DateCreated <= Globals.Date2
                                                                         , l => l.Window.Application)
                                              .ConfigureAwait(false);

            var totalDuration = (from l in logs
                                 group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                 select grp).ToList().Select(g => new { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day), Duration = (double)g.Sum(l => l.Duration) });


            var result = (from l in logs
                          where l.Window.Application.ApplicationID == app.ApplicationID
                          group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day, name = l.Window.Application.Name } into grp
                          select grp).ToList()
                               .Select(g => new TopAppsModel
                               {
                                   AppName = g.Key.name,
                                   Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                   DateTime = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                   Usage = g.Sum(l => l.Duration) / totalDuration.First(t => t.Date == new DateTime(g.Key.year, g.Key.month, g.Key.day)).Duration,
                                   Duration = g.Sum(l => l.Duration)
                               })
                               .OrderByDescending(t => t.DateTime)
                               .ToList();

            var requestedApp = result.Where(a => a.AppName == app.Name).FirstOrDefault();

            if (requestedApp != null)
                requestedApp.IsSelected = true;

            return result;
        }

        private async Task<List<TopWindowsModel>> GetTopWindowsAsync()
        {
            var topApps = TopAppsOverall;
            if (TopAppsList == null || topApps == null)
                return null;

            string appName = topApps.AppName;

            var days = TopAppsList.Where(t => t.IsSelected).Select(t => t.DateTime);
            var logs = _cachedLogs == null ? _cachedLogs = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                    && l.Window.Application.Name == appName
                                                                    , l => l.Window)
                                                                    .ConfigureAwait(false) : _cachedLogs;

            var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

            double totalDuration = totalFiltered.Sum(l => l.Duration);

            var result = (from l in totalFiltered
                          group l by l.Window.Title into grp
                          select grp).Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                                  .OrderByDescending(t => t.Duration)
                                  .ToList();

            return result;

        }

        private Task<List<TopAppsModel>> GetTopAppsOverallAsync()
        {
            return Task<List<TopAppsModel>>.Run(() =>
            {
                Aplication app = SelectedApplication;
                if (app == null)
                    return null;

                IEnumerable<Log> logs;
                using (var context = new AppsEntities())
                {

                    logs = (from u in context.Users.AsNoTracking()
                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && l.DateCreated >= Globals.Date1
                            && l.DateCreated <= Globals.Date2
                            select l).Include(l => l.Window.Application).ToList();
                }

                var totalDuration = (from l in logs
                                     group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                     select grp).ToList().Select(g => new { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day), Duration = (double)g.Sum(l => l.Duration) });


                var result = (from l in logs
                              where l.Window.Application.ApplicationID == app.ApplicationID
                              group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day, name = l.Window.Application.Name } into grp
                              select grp).ToList()
                                   .Select(g => new TopAppsModel
                                   {
                                       AppName = g.Key.name,
                                       Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                                       DateTime = new DateTime(g.Key.year, g.Key.month, g.Key.day)
                                       ,
                                       Usage = g.Sum(l => l.Duration) / totalDuration.First(t => t.Date == new DateTime(g.Key.year, g.Key.month, g.Key.day)).Duration
                                       ,
                                       Duration = g.Sum(l => l.Duration)
                                   })
                                   .OrderByDescending(t => t.DateTime)
                                   .ToList();

                var requestedApp = result.Where(a => a.AppName == app.Name).FirstOrDefault();

                if (requestedApp != null)
                    requestedApp.IsSelected = true;

                return result;
            });
        }

        private Task<List<TopWindowsModel>> GetTopWindowsOverallAsync()
        {
            return Task<List<TopWindowsModel>>.Run(() =>
            {
                var topApps = TopAppsOverall;
                if (TopAppsList == null || topApps == null)
                    return null;
                string appName = topApps.AppName;

                var days = TopAppsList.Where(t => t.IsSelected).Select(t => t.DateTime);
                IEnumerable<Log> logs;

                using (var context = new AppsEntities())
                {
                    logs = (from u in context.Users.AsNoTracking()
                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                            where u.UserID == Globals.SelectedUserID
                            && a.Name == appName
                            select l).Include(l => l.Window)
                                    .ToList();
                }

                var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

                double totalDuration = totalFiltered.Sum(l => l.Duration);

                var result = (from l in totalFiltered
                              group l by l.Window.Title into grp
                              select grp).Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                                      .OrderByDescending(t => t.Duration)
                                      .ToList();

                return result;

            });
        }

        private async Task<List<DailyWindowSeries>> GetChartContentAsync()
        {
            var topApps = TopAppsOverall;

            if (TopAppsList == null || topApps == null || TopWindowsList == null)
                return null;

            string appName = topApps.AppName;
            List<string> selectedWindows = TopWindowsList.Where(w => w.IsSelected).Select(w => w.Title).ToList();
            if (selectedWindows.Count == 0)
                return null;

            var days = TopAppsList.Where(t => t.IsSelected).Select(t => t.DateTime);

            var logs = _cachedLogs == null ? _cachedLogs = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                    && l.Window.Application.Name == appName
                                                                    , l => l.Window)
                                                                    .ConfigureAwait(false) : _cachedLogs;

            var totalFiltered = logs.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)) && selectedWindows.Contains(l.Window.Title));

            List<DailyWindowSeries> result = new List<DailyWindowSeries>();

            var projected = from l in totalFiltered
                            group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                            select grp;

            foreach (var grp in projected)
            {
                var projected2 = grp.GroupBy(g => g.Window.Title);
                DailyWindowSeries series = new DailyWindowSeries() { Date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day).ToShortDateString() };
                List<DailyWindowDurationModel> modelList = new List<DailyWindowDurationModel>();
                foreach (var grp2 in projected2)
                {
                    DailyWindowDurationModel model = new DailyWindowDurationModel() { Title = grp2.Key, Duration = Math.Round(new TimeSpan(grp2.Sum(l => l.Duration)).TotalMinutes, 2) };
                    modelList.Add(model);
                }
                series.DailyWindowCollection = modelList;
                result.Add(series);
            }

            return result;
        }

        #endregion

        #region Mediator Methods

        private void ApplicationAdded(Aplication app)
        {
            TopAppsList = null;
            TopWindowsList = null;
            ChartList = null;
            List<Aplication> copy = new List<Aplication>();
            copy.Add(app);
            copy = AplicationList.Union(copy).ToList();
            AplicationList = null;
            AplicationList = copy;
            _cachedLogs = null;
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
                        using (var context = new AppsEntities())
                        {
                            if (username == "All users")
                            {
                                foreach (var user in context.Users)
                                {
                                    foreach (var app in appList)
                                    {
                                        if (app.Description.ToLower() != "apps tracker" || !string.IsNullOrEmpty(app.WinName))
                                        {
                                            if (!context.AppsToBlocks.ItemExists(user, app))
                                            {
                                                AppsToBlock appToBlock = new AppsToBlock(user, app);
                                                context.AppsToBlocks.Add(appToBlock);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var app in appList)
                                {
                                    if (app.Description.ToLower() != "apps tracker" || !string.IsNullOrEmpty(app.WinName))
                                    {
                                        var uzer = context.Users.FirstOrDefault(u => u.Name == username);
                                        if (!context.AppsToBlocks.ItemExists(uzer, app))
                                        {
                                            AppsToBlock appToBlock = new AppsToBlock(uzer, app);
                                            context.AppsToBlocks.Add(appToBlock);
                                        }
                                    }
                                }
                            }
                            context.SaveChangesAsync();
                            var notifyList = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Include(a => a.Application).ToList();
                            Mediator.NotifyColleagues<List<AppsToBlock>>(MediatorMessages.AppsToBlockChanged, notifyList);
                        }
                    }
                }
            }
        }
        private void OverallAppSelectionChanged()
        {
            ChartVisible = false;

            if (TopAppsList == null)
                return;

            long ticks = 0;
            var filteredApps = TopAppsList.Where(t => t.IsSelected);

            if (filteredApps.Count() == 0)
            {
                TopWindowsList = null;
                return;
            }

            foreach (var app in filteredApps)
            {
                ticks += app.Duration;
            }


            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            OverallAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            LoadWindowsOverall();
        }
        private void OverallWindowSelectionChanged()
        {
            if (TopWindowsList == null)
            {
                ChartList = null;
                ChartVisible = false;
                return;
            }

            if (TopWindowsList.Where(w => w.IsSelected).Count() == 0)
            {
                ChartVisible = false;
                return;
            }

            ChartVisible = true;
            long ticks = 0;

            foreach (var window in TopWindowsList.Where(w => w.IsSelected))
                ticks += window.Duration;

            if (ticks == 0)
                return;

            TimeSpan timeSpan = new TimeSpan(ticks);
            OverallWindowDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            LoadChart();
        }

        private void GetAllUsers()
        {
            if (_allUsersList == null) _allUsersList = new List<MenuItem>();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "All users";
            _allUsersList.Add(menuItem);
            using (var context = new AppsEntities())
            {
                foreach (var user in context.Users)
                {
                    menuItem = new MenuItem();
                    menuItem.Header = user.Name;
                    _allUsersList.Add(menuItem);
                }
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
                    LoadAppsOverall();
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

        #region Override Methods

        protected override void Disposing()
        {
            this._selectedApplication = null;
            base.Disposing();
        }

        #endregion
    }
}

using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Utils;
using System.Collections.ObjectModel;

namespace Task_Logger_Pro.ViewModels
{
    class Data_singleSummaryViewModel : ViewModelBase, IChildVM
    {
        #region Fields

        bool _working;

        DateTime _selectedDate = DateTime.Today;

        string _singleAppDuration;
        string _singleWindowDuration;
        string _dayStart;
        string _dayEnd;

        Aplication _selectedApplication;

        TopAppsModel _topAppsSingle;

        List<TopAppsModel> _weakAppSingle; //new WeakReference(null);
        WeakReference _weakWindowSingle = new WeakReference(null);
        WeakReference _weakChart = new WeakReference(null);

        ICommand _singleAppSelectionChangedCommand;
        ICommand _singleWindowSelectionChangedCommand;
        ICommand _addDaysCommand;

        public event EventHandler<WorkerEventArgs> WorkerChanged;

        #endregion

        #region Properties
        public bool IsContentLoaded
        {
            get { return (_weakAppSingle != null); }
        }
        public bool IsActive { get; set; }
        public bool Working
        {
            get
            {
                return _working;
            }
            set
            {
                _working = value;
                var handler = WorkerChanged;
                if (handler != null)
                    handler(this, new WorkerEventArgs() { Working = value });
            }
        }
        public DateTime SelectedDate
        {
            get
            {
                return _selectedDate;
            }
            set
            {
                _selectedDate = value;
                PropertyChanging("SelectedDate");
                if (SelectedApplication != null)
                {
                    LoadAppsSingle();
                    LoadWindowsSingle();
                    LoadDayInfo();
                    LoadChart();
                }
            }
        }
        public string SingleAppDuration
        {
            get
            {
                return _singleAppDuration;
            }
            set
            {
                _singleAppDuration = value;
                PropertyChanging("SingleAppDuration");
            }
        }
        public string SingleWindowDuration
        {
            get
            {
                return _singleWindowDuration;
            }
            set
            {
                _singleWindowDuration = value;
                PropertyChanging("SingleWindowDuration");
            }
        }
        public string DayStart
        {
            get
            {
                return _dayStart;
            }
            set
            {
                _dayStart = value;
                PropertyChanging("DayStart");
            }
        }
        public string DayEnd
        {
            get
            {
                return _dayEnd;
            }
            set
            {
                _dayEnd = value;
                PropertyChanging("DayEnd");
            }
        }
        public string Title
        {
            get { return "single day"; }
        }
        public TopAppsModel TopAppsSingle
        {
            get
            {
                return _topAppsSingle;
            }
            set
            {
                _topAppsSingle = value;
                SingleWindowDuration = string.Empty;
                if (value != null)
                    LoadWindowsSingle();
                PropertyChanging("TopAppsSingle");
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
                if (value != null)
                {
                    LoadContent();
                }
            }
        }
        public List<TopAppsModel> WeakAppSingle
        {
            get
            {
                return _weakAppSingle;
            }
            set
            {
                _weakAppSingle = value;
            }
        }
        public WeakReference WeakWindowSingle
        {
            get
            {
                return _weakWindowSingle;
            }
        }
        public WeakReference WeakChart
        {
            get
            {
                return _weakChart;
            }
        }
        public ICommand SingleAppSelectionChangedCommand
        {
            get
            {
                return _singleAppSelectionChangedCommand == null ? _singleAppSelectionChangedCommand = new DelegateCommand(SingleAppSelectionChanged) : _singleAppSelectionChangedCommand;
            }
        }
        public ICommand SingleWindowSelectionChangedCommand
        {
            get
            {
                return _singleWindowSelectionChangedCommand == null ? _singleWindowSelectionChangedCommand = new DelegateCommand(SingleWindowSelectionChanged) : _singleWindowSelectionChangedCommand;
            }
        }
        public ICommand AddDaysCommand
        {
            get { return _addDaysCommand == null ? _addDaysCommand = new DelegateCommand(AddDays) : _addDaysCommand; }
        }

        #endregion

        #region Load Methods

        public void LoadContent()
        {
           
            if (!IsActive)
                return;
            SingleAppDuration = string.Empty;
            SingleWindowDuration = string.Empty;
            LoadAppsSingle();
            LoadDayInfo();
            LoadChart();
        }

        private async void LoadAppsSingle()
        {
            Working = true;
            WeakAppSingle = await GetTopAppsSingleAsync();
            // System.Diagnostics.Debug.Assert(WeakAppSingle.Target != null, "TARGET NULL");
            Working = false;
            PropertyChanging("WeakAppSingle");
        }

        private async void LoadWindowsSingle()
        {
            Working = true;
            WeakWindowSingle.Target = await GetTopWindowsSingleAsync();
            Working = false;
            PropertyChanging("WeakWindowSingle");
        }
        private async void LoadDayInfo()
        {
            Working = true;
            Tuple<string, string> info = await GetDayInfoAsync();
            DayStart = info.Item1;
            DayEnd = info.Item2;
            Working = false;
        }
        private async void LoadChart()
        {
            if (SelectedApplication == null)
                return;
            Working = true;
            WeakChart.Target = await GetChartContentAsync();
            Working = false;
            PropertyChanging("WeakChart");
        }

        private Task<List<TopAppsModel>> GetTopAppsSingleAsync()
        {
            return Task<List<TopAppsModel>>.Run(() =>
            {
                if (SelectedApplication == null)
                    return null;
                string appName = SelectedApplication.Name;
                var nextDay = SelectedDate.AddDays(1);

                using (var context = new AppsEntities1())
                {
                    double totalDuration = (from u in context.Users.AsNoTracking()
                                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                            where u.UserID == Globals.SelectedUserID
                                            && l.DateCreated >= SelectedDate
                                            && l.DateCreated <= nextDay
                                            select (double?)l.Duration).Sum() ?? 0;

                    //var result = (from u in context.Users.AsNoTracking()
                    //              join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //              join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //              join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //              where u.UserID == Globals.SelectedUserID
                    //              && l.DateCreated >= SelectedDate
                    //              && l.DateCreated <= nextDay
                    //              group l by l.Window.Application.Name into grp
                    //              select grp).ToList()
                    //                   .Select(g => new TopAppsModel { AppName = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                    //                   .OrderByDescending(t => t.Duration)
                    //                   .ToList();

                    var result = (from u in context.Users.AsNoTracking()
                                  join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                  join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                  join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                  where u.UserID == Globals.SelectedUserID
                                  && l.DateCreated >= SelectedDate
                                  && l.DateCreated <= nextDay
                                  && a.ApplicationID == SelectedApplication.ApplicationID
                                  group l by l.Window.Application.Name into grp
                                  select grp).ToList()
                                     .Select(g => new TopAppsModel { AppName = g.Key, Date = SelectedDate.ToShortDateString(), Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                        //.OrderByDescending(t => t.Duration)
                                     .ToList();

                    var requestedApp = result.Where(a => a.AppName == appName).FirstOrDefault();

                    if (requestedApp == null)
                        result.Add(new TopAppsModel() { AppName = appName, Date = SelectedDate.ToShortDateString(), DateTime = SelectedDate, Duration = 0, Usage = 0, IsSelected = true });
                    else
                        requestedApp.IsSelected = true;

                    return result;

                }
            });
        }

        private Task<List<TopWindowsModel>> GetTopWindowsSingleAsync()
        {
            return Task<List<TopWindowsModel>>.Run(() =>
            {
                if (TopAppsSingle == null)
                    return null;
                string appName = TopAppsSingle.AppName;
                var nextDay = SelectedDate.AddDays(1);
                using (var context = new AppsEntities1())
                {
                    var total = (from u in context.Users.AsNoTracking()
                                 join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                 join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                 join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                 where u.UserID == Globals.SelectedUserID
                                 && a.Name == appName
                                 && l.DateCreated >= SelectedDate
                                 && l.DateCreated <= nextDay
                                 select l).ToList();

                    double totalDuration = total.Sum(l => l.Duration);

                    var result = (from l in total
                                  group l by l.Window.Title into grp
                                  select grp)
                                          .Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                                          .OrderByDescending(t => t.Duration)
                                          .ToList();
                    return result;

                    //double totalDuration = (from u in context.Users.AsNoTracking()
                    //                        join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //                        join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //                        join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //                        where u.UserID == Globals.SelectedUserID
                    //                        && a.Name == appName
                    //                        && l.DateCreated >= SelectedDate
                    //                        && l.DateCreated <= nextDay
                    //                        select (double?)l.Duration).Sum() ?? 0;

                    //var result = (from u in context.Users.AsNoTracking()
                    //              join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //              join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //              join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //              where u.UserID == Globals.SelectedUserID
                    //              && a.Name == appName
                    //              && l.DateCreated >= SelectedDate
                    //              && l.DateCreated <= nextDay
                    //              group l by l.Window.Title into grp
                    //              select grp).ToList()
                    //                      .Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                    //                      .OrderByDescending(t => t.Duration)
                    //                      .ToList();
                    //return result;
                }
            });
        }

        private Task<Tuple<string, string>> GetDayInfoAsync()
        {
            return Task<Tuple<string, string>>.Run(() =>
            {
                DateTime today = SelectedDate.Date;
                DateTime nextDay = today.AddDays(1d);

                using (var context = new AppsEntities1())
                {
                    string loginType = UsageTypes.Login.ToString();

                    var loginBegin = (from u in context.Users.AsNoTracking()
                                      join l in context.Usages on u.UserID equals l.UserID
                                      where l.UsageStart >= today
                                      && l.UsageStart <= nextDay
                                      && l.UserID == Globals.SelectedUserID
                                      && l.UsageType.UType == loginType
                                      orderby l.UsageStart ascending
                                      select l).Take(1).ToList().FirstOrDefault();

                    var loginEnd = (from u in context.Users.AsNoTracking()
                                    join l in context.Usages on u.UserID equals l.UserID
                                    where l.UsageStart >= today
                                    && l.UsageStart <= nextDay
                                    && l.UserID == Globals.SelectedUserID
                                    && l.UsageType.UType == loginType
                                    orderby l.UsageStart descending
                                    select l).Take(1).ToList().FirstOrDefault();

                    string dayBegin = loginBegin == null ? "N/A" : loginBegin.UsageStart.ToShortTimeString();
                    string dayEnd = loginEnd == null ? "N/A" : loginEnd.UsageEnd.ToShortTimeString();
                    return new Tuple<string, string>(dayBegin, dayEnd);
                }

            });
        }


        private Task<List<DailyTopWindowSeries>> GetChartContentAsync()
        {
            return Task<List<DailyTopWindowSeries>>.Run(() =>
            {
                if (SelectedApplication == null)
                    return null;
                string appName = SelectedApplication.Name;
                var nextDay = SelectedDate.AddDays(1);

                using (var context = new AppsEntities1())
                {
                    double totalDuration = (from u in context.Users.AsNoTracking()
                                            join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                            join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                            join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                            where u.UserID == Globals.SelectedUserID
                                            && a.Name == appName
                                            && l.DateCreated >= SelectedDate
                                            && l.DateCreated <= nextDay
                                            select (double?)l.Duration).Sum() ?? 0;

                    var result = (from u in context.Users.AsNoTracking()
                                  join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                  join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                  join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                  where u.UserID == Globals.SelectedUserID
                                  && a.Name == appName
                                  && l.DateCreated >= SelectedDate
                                  && l.DateCreated <= nextDay
                                  group l by l.Window.Title into grp
                                  select grp).ToList()
                                          .Select(g => new TopWindowsModel { Title = g.Key, Usage = Math.Round((g.Sum(l => l.Duration) / totalDuration) * 100d, 0), Duration = (long)new TimeSpan(g.Sum(l => l.Duration)).TotalMinutes })
                                          .OrderByDescending(t => t.Duration)
                                          .Take(5)
                                          .ToList();
                    List<DailyTopWindowSeries> returnList = new List<DailyTopWindowSeries>();
                    DailyTopWindowSeries series = new DailyTopWindowSeries() { Date = SelectedDate.ToShortDateString() };
                    series.DailyUsageTypeCollection = new ObservableCollection<TopWindowsModel>(result);
                    returnList.Add(series);

                    return returnList;
                }
            });

        }

        #endregion

        #region Commmand Methods

        private void SingleAppSelectionChanged()
        {
            IEnumerable<TopAppsModel> topAppCollection = WeakAppSingle as IEnumerable<TopAppsModel>;
            if (topAppCollection == null)
                return;
            long ticks = 0;
            foreach (var app in topAppCollection)
            {
                if (app.IsSelected)
                    ticks += app.Duration;
            }
            if (ticks == 0)
                return;
            TimeSpan timeSpan = new TimeSpan(ticks);
            SingleAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        private void SingleWindowSelectionChanged()
        {
            IEnumerable<TopWindowsModel> topWindowCollection = WeakWindowSingle.Target as IEnumerable<TopWindowsModel>;
            if (topWindowCollection == null)
                return;
            long ticks = 0;
            foreach (var window in topWindowCollection)
            {
                if (window.IsSelected)
                    ticks += window.Duration;
            }
            if (ticks == 0)
                return;
            TimeSpan timeSpan = new TimeSpan(ticks);
            SingleWindowDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        private void AddDays(object parameter)
        {
            string stringParameter = parameter as string;
            if (stringParameter == null)
                return;
            switch (stringParameter)
            {
                case "+":
                    SelectedDate = SelectedDate.AddDays(1d);
                    break;
                case "-":
                    SelectedDate = SelectedDate.AddDays(-1d);
                    break;
                default:
                    SelectedDate = DateTime.Today;
                    break;
            }
        }

        #endregion

        #region Override Methods
        protected override void Disposing()
        {
            base.Disposing();
        }
        #endregion
    }
}

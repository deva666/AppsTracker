using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows.Input;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Utils;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;

namespace Task_Logger_Pro.ViewModels
{
    class Data_overallSummaryViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        bool _working;

        string _overallAppDuration;
        string _overallWindowDuration;

        WeakReference _weakAppOverall = new WeakReference(null);
        WeakReference _weakWindowOverall = new WeakReference(null);

        Aplication _selectedApplication;

        TopAppsModel _topAppsOverall;

        ICommand _overallAppSelectionChangedCommand;
        ICommand _overallWindowSelectionChangedCommand;

        public event EventHandler<WorkerEventArgs> WorkerChanged;

        #endregion

        #region Properties
        public bool IsContentLoaded
        {
            get { return (_weakAppOverall.Target != null || _weakWindowOverall.Target != null); }
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
                var handler = WorkerChanged;
                if (handler != null)
                    handler(this, new WorkerEventArgs() { Working = value });

            }
        }
        public bool IsActive
        {
            get;
            set;
        }
        public string Title
        {
            get { return "date range"; }
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
                return Globals.Date1;
            }
        }
        public DateTime Date2
        {
            get
            {
                return Globals.Date2;
            }
        }
        public WeakReference WeakAppOverall
        {
            get
            {
                return _weakAppOverall;
            }
        }
        public WeakReference WeakWindowOverall
        {
            get
            {
                return _weakWindowOverall;
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
                if (value != null)
                    LoadWindowsOverall(value.AppName);
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

        public Mediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        #endregion

        #region Constructor

        public Data_overallSummaryViewModel()
        {
            //Mediator.Register(MediatorMessages.FilterDatesChanged, new Action(RefreshDates));
        }

        private void RefreshDates()
        {
            PropertyChanging("Date1");
            PropertyChanging("Date2");
        }

        #endregion

        #region Load Methods

        public void LoadContent()
        {
            if (!IsActive)
                return;
            OverallAppDuration = string.Empty;
            OverallWindowDuration = string.Empty;
            if (SelectedApplication != null)
                LoadAppsOverall(SelectedApplication.Name);
        }
        private async void LoadAppsOverall(string appName)
        {
            Working = true;
            WeakAppOverall.Target = await GetTopAppsOverallAsync(appName);
            Working = false;
            PropertyChanging("WeakAppOverall");
        }
        private async void LoadWindowsOverall(string appName)
        {
            Working = true;
            WeakWindowOverall.Target = await GetTopWindowsOverallAsync();
            Working = false;
            PropertyChanging("WeakWindowOverall");
        }
        private Task<List<TopAppsModel>> GetTopAppsOverallAsync(string appName)
        {
            return Task<List<TopAppsModel>>.Run(() =>
            {
                using (var context = new AppsEntities())
                {
                    var totalDuration = (from u in context.Users.AsNoTracking()
                                         join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                         join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                         join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                         where u.UserID == Globals.SelectedUserID
                                         && l.DateCreated >= Globals.Date1
                                         && l.DateCreated <= Globals.Date2
                                         group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                         select grp).ToList().Select(g => new { Date = new DateTime(g.Key.year, g.Key.month, g.Key.day), Duration = (double)g.Sum(l => l.Duration) });

                    //select (double?)l.Duration).Sum() ?? 0;

                    //var mostUsedApps = (from u in context.Users.AsNoTracking()
                    //                    join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //                    join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //                    join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //                    where u.UserID == Globals.SelectedUserID
                    //                    && l.DateCreated >= Globals.Date1
                    //                    && l.DateCreated <= Globals.Date2
                    //                    group l by l.Window.Application.Name into grp
                    //                    select grp).ToList()
                    //                                .OrderByDescending(l => l.Sum(log => log.Duration))
                    //                                .Select(g => g.Key)
                    //    //.Take(5)
                    //                                .ToList();
                    //mostUsedApps.Add(appName);

                    var result = (from u in context.Users.AsNoTracking()
                                  join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                  join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                  join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                  where u.UserID == Globals.SelectedUserID
                                      //  && mostUsedApps.Contains(a.Name)
                                  && l.DateCreated >= Globals.Date1
                                  && l.DateCreated <= Globals.Date2
                                  && a.ApplicationID == SelectedApplication.ApplicationID
                                  group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day, name = a.Name } into grp
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
                                       .OrderByDescending(t => t.Duration)
                                       .ToList();

                    var requestedApp = result.Where(a => a.AppName == appName).FirstOrDefault();
                    if (requestedApp == null)
                        return result;
                    else
                    {
                        requestedApp.IsSelected = true;
                        return result;
                    }

                }
            });
        }

        private Task<List<TopWindowsModel>> GetTopWindowsOverallAsync()
        {
            return Task<List<TopWindowsModel>>.Run(() =>
            {
                List<TopAppsModel> topAppsList = WeakAppOverall.Target as List<TopAppsModel>;
                var topApps = TopAppsOverall;
                if (topAppsList == null || topApps == null)
                    return null;
                string appName = topApps.AppName;
             
                var days = topAppsList.Where(t => t.IsSelected).Select(t => t.DateTime);

                using (var context = new AppsEntities())
                {
                    var total = (from u in context.Users.AsNoTracking()
                                 join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                                 join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                                 join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                                 where u.UserID == Globals.SelectedUserID
                                 && a.Name == appName
                                 select l).Include(l => l.Window)
                                    .ToList();

                    var totalFiltered = total.Where(l => days.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

                    double totalDuration = totalFiltered.Sum(l => l.Duration);

                    var result = (from l in totalFiltered
                                  group l by l.Window.Title into grp
                                  select grp).Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                                          .OrderByDescending(t => t.Duration)
                                          .ToList();

                    //double totalDuration = (from u in context.Users.AsNoTracking()
                    //                        join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //                        join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //                        join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //                        where u.UserID == Globals.SelectedUserID
                    //                        && a.Name == appName
                    //                        && l.DateCreated >= day1
                    //                        && l.DateCreated <= day2
                    //                        select (double?)l.Duration).Sum() ?? 0;

                    //var mostUsedWindows = (from u in context.Users.AsNoTracking()
                    //                       join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //                       join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //                       join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //                       where u.UserID == Globals.SelectedUserID
                    //                          && l.DateCreated >= Globals.Date1
                    //        && l.DateCreated <= Globals.Date2
                    //                       && a.Name == appName
                    //                       group l by l.Window.Title into grp
                    //                       select grp).ToList()
                    //                                .OrderByDescending(l => l.Sum(log => log.Duration))
                    //                                .Select(g => g.Key)
                    //                                .Take(5)
                    //                                .ToList();

                    //var result = (from u in context.Users.AsNoTracking()
                    //              join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    //              join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    //              join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    //              where u.UserID == Globals.SelectedUserID
                    //              && a.Name == appName
                    //                  //  && mostUsedWindows.Contains(w.Title)
                    //              && l.DateCreated >= day1
                    //              && l.DateCreated <= day2
                    //              group l by l.Window.Title into grp
                    //              select grp).ToList()
                    //                      .Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                    //                      .OrderByDescending(t => t.Duration)
                    //                      .ToList();
                    return result;
                }
            });
        }

        #endregion

        #region Command Methods

        private void OverallAppSelectionChanged()
        {
            IEnumerable<TopAppsModel> topAppCollection = WeakAppOverall.Target as IEnumerable<TopAppsModel>;
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
            OverallAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            if (TopAppsOverall != null)
                LoadWindowsOverall(TopAppsOverall.AppName);
        }
        private void OverallWindowSelectionChanged()
        {
            IEnumerable<TopWindowsModel> topWindowCollection = WeakWindowOverall.Target as IEnumerable<TopWindowsModel>;
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
            OverallWindowDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        #endregion

    }
}

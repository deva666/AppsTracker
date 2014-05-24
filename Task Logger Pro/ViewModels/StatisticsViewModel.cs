//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;
//using Task_Logger_Pro.Controls;
//using Task_Logger_Pro.Logging;
//using Task_Logger_Pro.Models;
//using Task_Logger_Pro.MVVM;

//namespace Task_Logger_Pro.Pages.ViewModels
//{
//    public class StatisticsViewModel : ViewModelBase, IPageViewModel
//    {
//        #region Fields

//        const string _pageSource = "/Pages/Statistics.xaml";
//        const string _pageName = "Statistics";

//        bool _working;
//        List<MostUsedAppModel> _mostUsedAppsCollection;
//        List<AllUsersModel> _allUsersCollection;
//        List<BlockedAppModel> _blockedAppsCollection;
//        List<KeystrokeModel> _keystrokesCollection;
//        List<ScreenshotModel> _screenshotsCollection;
//        List<UsageModel> _usageCollection;
//        List<DailyAppModel> _dailyAppCollection;
//        List<DailyUsedAppsSeries> _dailyUsedAppsSeries;
//        List<DailyBlockedAppModel> _dailyBlockedAppCollection;
//        List<DailyKeystrokeModel> _dailyKeystrokesCollection;
//        List<DailyScreenshotModel> _dailyScreenshotsCollection;
//        List<FilewatcherModel> _filewatcherCollection;

//        object _selectedItem;
//        MostUsedAppModel _mostUsedAppModel;
//        AllUsersModel _allUsersModel;
//        BlockedAppModel _blockedAppModel;
//        KeystrokeModel _keystrokeModel;
//        ScreenshotModel _screenshotModel;
//        UsageModel _usageModel;

//        ICommand _returnFromDetailedViewCommand;

//        #endregion

//        #region Properties

//        public bool Working { get { return _working; } set { lock (this) _working = value; PropertyChanging("Working"); } }
//        public string PageSource { get { return _pageSource; } }
//        public string PageName { get { return _pageName; } }
//        public DateTime FirstDate { get { return Globals.Date1; } }
//        public DateTime LastDate { get { return Globals.Date2; } }
//        public UzerSetting UserSettings { get { return App.UzerSetting; } }
//        public Uzer User { get { return Globals.SelectedUser; } }

//        public IEnumerable<MostUsedAppModel> MostUsedAppsCollection { get { return _mostUsedAppsCollection; } }
//        public IEnumerable<AllUsersModel> AllUsersCollection { get { return _allUsersCollection; } }
//        public IEnumerable<BlockedAppModel> BlockedAppsCollection { get { return _blockedAppsCollection; } }
//        public IEnumerable<KeystrokeModel> KeystrokesCollection { get { return _keystrokesCollection; } }
//        public IEnumerable<ScreenshotModel> ScreenshotsCollection { get { return _screenshotsCollection; } }
//        public IEnumerable<UsageModel> UsageCollection { get { return _usageCollection; } }
//        //public IEnumerable<DailyAppModel> DailyAppCollection { get { return _dailyAppCollection; } }
//        public IEnumerable<DailyUsedAppsSeries> DailyUsedAppsSeries { get { return _dailyUsedAppsSeries; } }
//        //public IEnumerable<DailyBlockedAppModel> DailyBlockedAppCollection { get { return _dailyBlockedAppCollection; } }
//        //public IEnumerable<DailyKeystrokeModel> DailyKeystrokesCollection { get { return _dailyKeystrokesCollection; } }
//        //public IEnumerable<DailyScreenshotModel> DailyScreenshotsCollection { get { return _dailyScreenshotsCollection; } }
//        public IEnumerable<FilewatcherModel> FilewatcherCollection { get { return _filewatcherCollection; } }

//        //public IEnumerable<MostUsedAppModel> MostUsedAppsCollection { get { LoadMostUsedApps(); return _mostUsedAppsCollection; } }
//        //public IEnumerable<AllUsersModel> AllUsersCollection { get { LoadAllUsers(); return _allUsersCollection; } }
//        //public IEnumerable<BlockedAppModel> BlockedAppsCollection { get { LoadBlockedApps(); return _blockedAppsCollection; } }
//        //public IEnumerable<KeystrokeModel> KeystrokesCollection { get { LoadKeystrokes(); return _keystrokesCollection; } }
//        //public IEnumerable<ScreenshotModel> ScreenshotsCollection { get { LoadScreenshots(); return _screenshotsCollection; } }
//        //public IEnumerable<UsageModel> UsageCollection { get { LoadUsage(); return _usageCollection; } }
//        // public IEnumerable<DailyAppModel> DailyAppCollection { get { LoadDailyAppUsage(); return _dailyAppCollection; } }



//        //public IEnumerable<DailyUsedAppsSeries> DailyUsedAppsSeries { get { LoadDailyApps(); return _dailyUsedAppsSeries; } }
//        //public IEnumerable<DailyBlockedAppModel> DailyBlockedAppCollection { get { LoadDailyBlockedApps(); return _dailyBlockedAppCollection; } }
//        //public IEnumerable<DailyKeystrokeModel> DailyKeystrokesCollection { get { LoadDailyKeystrokes(); return _dailyKeystrokesCollection; } }
//        //public IEnumerable<DailyScreenshotModel> DailyScreenshotsCollection { get { LoadDailyScreenshots(); return _dailyScreenshotsCollection; } }
//        //public IEnumerable<FilewatcherModel> FilewatcherCollection { get { LoadFilewatchers(); return _filewatcherCollection; } }

//        public object SelectedItem { get { return _selectedItem; } set { _selectedItem = value; PropertyChanging("SelectedItem"); } }
//        public UsageModel UsageModel { get { return _usageModel; } set { _usageModel = value; PropertyChanging("UsageModel"); } }
//        public ScreenshotModel ScreenshotModel { get { return _screenshotModel; } set { _screenshotModel = value; PropertyChanging("ScreenshotModel"); PropertyChanging("DailyScreenshotsCollection"); } }
//        public KeystrokeModel KeystrokeModel { get { return _keystrokeModel; } set { _keystrokeModel = value; PropertyChanging("KeystrokeModel"); PropertyChanging("DailyKeystrokesCollection"); } }
//        public BlockedAppModel BlockedAppModel { get { return _blockedAppModel; } set { _blockedAppModel = value; PropertyChanging("BlockedAppModel"); PropertyChanging("DailyBlockedAppCollection"); } }
//        public AllUsersModel AllUsersModel { get { return _allUsersModel; } set { _allUsersModel = value; PropertyChanging("AllUsersModel"); } }
//        public MostUsedAppModel MostUsedAppModel { get { return _mostUsedAppModel; } set { _mostUsedAppModel = value; PropertyChanging("MostUsedAppModel"); PropertyChanging("DailyAppCollection"); } }
//        public ICommand ReturnFromDetailedViewCommand { get { _returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView); return _returnFromDetailedViewCommand; } }

//        #endregion

//        #region Constructor

//        public StatisticsViewModel()
//        {
//            //Working = true;
//            Mediator.Register(MediatorMessages.RefreshLogs, new Action<object>((u) => { LoadAll(); PropertyChanging(""); }));
//            //GetAll();
//            LoadAll();
//        }

//        #endregion

//        #region Parallel Methods
//        private void LoadAll()
//        {
//            //Task.Factory.StartNew(GetAllUsers);
//            //Task.Factory.StartNew(GetUsage);
//            //Task.Factory.StartNew(GetBlockedApps);
//            //Task.Factory.StartNew(GetScreenshots);
//            //Task.Factory.StartNew(GetKeystrokes);
//            //Task.Factory.StartNew(GetMostUsedApps);
//            //Task.Factory.StartNew(GetFilewatchers);
//            //Task.Factory.StartNew(GetDailyApps);

//            //Task.Factory.StartNew(GetAllUsers);
//            //Task.Factory.StartNew(GetUsage);
//            //Task.Factory.StartNew(GetBlockedApps);
//            //Task.Factory.StartNew(GetScreenshots);
//            //Task.Factory.StartNew(GetKeystrokes);
//            //Task.Factory.StartNew(GetMostUsedApps);
//            //Task.Factory.StartNew(GetFilewatchers);
//            //Task.Factory.StartNew(GetDailyApps);
//        }
//        private void GetAll()
//        {
//            using (new WaitCursor())
//            {
//                Parallel.Invoke(GetMostUsedApps, GetAllUsers, GetFilewatchers, GetKeystrokes, GetScreenshots, GetUsage, GetDailyApps, GetBlockedApps);
//            }
//        }

//        private void GetMostUsedApps()
//        {

//            using (var context = new AppsEntities1())
//            {
//                _mostUsedAppsCollection = (from u in context.Users.AsNoTracking().ToList()
//                                           join a in context.Applications.AsNoTracking().ToList() on u.UserID equals a.UserID
//                                           join w in context.Windows.AsNoTracking().ToList() on a.ApplicationID equals w.ApplicationID
//                                           join l in context.Logs.AsNoTracking().ToList() on w.WindowID equals l.WindowID
//                                           where u.UserID == Globals.SelectedUserID
//                                           && l.DateCreated >= Globals.Date1
//                                           && l.DateCreated <= Globals.Date2
//                                           && a.Name != Constants.IDLE_APP_NAME
//                                           group l by l.AppName into g
//                                           select new MostUsedAppModel { AppName = g.Key, Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetAllUsers()
//        {
//            Working = true;
//            using (var context = new AppsEntities1())
//            {
//                _allUsersCollection = (from l in context.Logins.AsNoTracking().ToList()
//                                       where l.LoginDate >= Globals.Date1
//                                       && l.LoginDate <= Globals.Date2
//                                       group l by l.User.Name into g
//                                       select new AllUsersModel { Username = g.Key, LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1) }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetKeystrokes()
//        {
//            using (var context = new AppsEntities1())
//            {
//                _keystrokesCollection = (from u in context.Users.AsNoTracking().ToList()
//                                         join a in context.Applications.AsNoTracking().ToList() on u.UserID equals a.UserID
//                                         join w in context.Windows.AsNoTracking().ToList() on a.ApplicationID equals w.ApplicationID
//                                         join l in context.Logs.AsNoTracking().ToList() on w.WindowID equals l.WindowID
//                                         where u.UserID == Globals.SelectedUserID
//                                         && l.DateCreated >= Globals.Date1
//                                         && l.DateCreated <= Globals.Date2
//                                         && a.Name != Constants.IDLE_APP_NAME
//                                         && l.HasKeystrokes
//                                         group l by l.AppName into g
//                                         select new KeystrokeModel { AppName = g.Key, Count = g.Sum(l => l.Keystrokes.Length) }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetScreenshots()
//        {
//            using (var context = new AppsEntities1())
//            {
//                _screenshotsCollection = (from u in context.Users.AsNoTracking().ToList()
//                                          join a in context.Applications.AsNoTracking().ToList() on u.UserID equals a.UserID
//                                          join w in context.Windows.AsNoTracking().ToList() on a.ApplicationID equals w.ApplicationID
//                                          join l in context.Logs.AsNoTracking().ToList() on w.WindowID equals l.WindowID
//                                          join s in context.Screenshots.AsNoTracking().ToList() on l.LogID equals s.LogID
//                                          where u.UserID == Globals.SelectedUserID
//                                          && s.Date >= Globals.Date1
//                                          && s.Date <= Globals.Date2
//                                          && a.Name != Constants.IDLE_APP_NAME
//                                          group s by s.AppName into g
//                                          select new ScreenshotModel { AppName = g.Key, Count = g.Count() }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetUsage()
//        {
//            Working = true;
//            using (var context = new AppsEntities1())
//            {
//                _usageCollection = (from l in context.Logins.AsNoTracking().ToList()
//                                    where l.UserID == Globals.SelectedUserID
//                                    && l.LoginDate >= Globals.Date1
//                                    && l.LoginDate <= Globals.Date2
//                                    group l by new { date = new DateTime(l.LoginDate.Year, l.LoginDate.Month, l.LoginDate.Day) } into g
//                                    select new UsageModel
//                                    {
//                                        Date = g.Key.date.ToShortDateString(),
//                                        Count = Math.Round(g.Sum(l => l.Duration.TotalHours), 1)
//                                    }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetFilewatchers()
//        {
//            using (var context = new AppsEntities1())
//            {
//                _filewatcherCollection = (from f in context.FileLogs.AsNoTracking().ToList()
//                                          where f.UserID == Globals.SelectedUserID
//                                          && f.Date >= FirstDate
//                                          && f.Date <= LastDate
//                                          group f by f.Event into g
//                                          select new FilewatcherModel { Event = g.Key, Count = g.Count() }).ToList();
//            }
//            PropertyChanging("");
//        }

//        private void GetDailyApps()
//        {
//            List<DailyUsedAppsSeries> dailyUsedAppsSeriesTemp = new List<DailyUsedAppsSeries>();
//            using (var context = new AppsEntities1())
//            {

//                var dailyApps = from u in context.Users.AsNoTracking().ToList()
//                                join a in context.Applications.AsNoTracking().ToList() on u.UserID equals a.UserID
//                                join w in context.Windows.AsNoTracking().ToList() on a.ApplicationID equals w.ApplicationID
//                                join l in context.Logs.AsNoTracking().ToList() on w.WindowID equals l.WindowID
//                                where u.UserID == Globals.SelectedUserID
//                                && l.DateCreated >= Globals.Date1
//                                && l.DateCreated <= Globals.Date2
//                                group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day), name = l.AppName } into g
//                                orderby g.Key.date
//                                select new
//                                {
//                                    Date = g.Key.date,
//                                    AppName = g.Key.name,
//                                    Duration = g.Sum(l => l.Duration)
//                                };

//                ObservableCollection<MostUsedAppModel> dailyUsedAppsCollection;
//                foreach (var app in dailyApps)
//                {
//                    if (!dailyUsedAppsSeriesTemp.Exists(d => d.Date == app.Date.ToShortDateString()))
//                    {
//                        dailyUsedAppsCollection = new ObservableCollection<MostUsedAppModel>();
//                        dailyUsedAppsCollection.Add(new Models.MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
//                        dailyUsedAppsSeriesTemp.Add(new Models.DailyUsedAppsSeries() { Date = app.Date.ToShortDateString(), DailyUsedAppsCollection = dailyUsedAppsCollection });
//                    }
//                    else
//                    {
//                        dailyUsedAppsSeriesTemp.First(d => d.Date == app.Date.ToShortDateString()).DailyUsedAppsCollection.Add(new MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
//                    }
//                }
//            }
//            _dailyUsedAppsSeries = dailyUsedAppsSeriesTemp;
//            PropertyChanging("DailyUsedAppsSeries");
//            Working = false;
//        }

//        private void GetBlockedApps()
//        {
//            using (var context = new AppsEntities1())
//            {
//                _blockedAppsCollection = (from b in context.BlockedApps.AsNoTracking().ToList()
//                                          where b.UserID == Globals.SelectedUserID
//                                          && b.Date >= Globals.Date1
//                                          && b.Date <= Globals.Date2
//                                          group b by b.Application.Name into g
//                                          select new BlockedAppModel { AppName = g.Key, Count = g.Count() }).ToList();
//            }
//            PropertyChanging("");
//        }
//        #endregion

//        #region Mediator Methods

//        private void Refresh()
//        {
//            PropertyChanging("FirstDate");
//            PropertyChanging("LastDate");
//            PropertyChanging("User");
//            PropertyChanging("ProcessLogCollection");
//            PropertyChanging("MostUsedAppsCollection");
//            PropertyChanging("AllUsersCollection");
//            PropertyChanging("BlockedAppsCollection");
//            PropertyChanging("KeystrokesCollection");
//            PropertyChanging("ScreenshotsCollection");
//            PropertyChanging("UsageCollection");
//            PropertyChanging("DailyAppCollection");
//            PropertyChanging("DailyUsedAppsSeries");
//            PropertyChanging("DailyBlockedAppCollection");
//            PropertyChanging("DailyKeystrokesCollection");
//            PropertyChanging("DailyScreenshotsCollection");
//            PropertyChanging("FilewatcherCollection");
//        }

//        #endregion

//        #region Command Methods

//        private void ReturnFromDetailedView()
//        {
//            UsageModel = null;
//            ScreenshotModel = null;
//            KeystrokeModel = null;
//            BlockedAppModel = null;
//            AllUsersModel = null;
//            MostUsedAppModel = null;
//            SelectedItem = null;
//        }

//        #endregion

//        #region Class Methods

//        //private void LoadMostUsedApps()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_mostUsedAppsCollection = new List<MostUsedAppModel>();
//        //    Task<List<MostUsedAppModel>> task = Task.Factory.StartNew<List<MostUsedAppModel>>(() =>
//        //        {
//        //            return (from u in App.DataAccess.UzerCollection
//        //                    join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                    join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                    join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                    where u.UserID == Globals.SelectedUserID
//        //                    && l.DateCreated >= Globals.Date1
//        //                    && l.DateCreated <= Globals.Date2
//        //                    && a.Name != Constants.IDLE_APP_NAME
//        //                    group l by l.AppName into g
//        //                    select new MostUsedAppModel { AppName = g.Key, Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) }).ToList();
//        //        });
//        //    _mostUsedAppsCollection = task.Result;
//        //    //foreach (var app in apps)
//        //    //{
//        //    //    _mostUsedAppsCollection.Add(new MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Dur).TotalHours, 1) });
//        //    //}
//        //}

//        //private void LoadAllUsers()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_allUsersCollection = new List<AllUsersModel>();
//        //    Task<List<AllUsersModel>> task = Task.Factory.StartNew<List<AllUsersModel>>(() =>
//        //        {
//        //            return (from l in App.DataAccess.LoginCollection
//        //                    where l.LoginDate >= Globals.Date1
//        //                    && l.LoginDate <= Globals.Date2
//        //                    group l by l.User.Name into g
//        //                    select new AllUsersModel { Username = g.Key, LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1) }).ToList();
//        //        });
//        //    _allUsersCollection = task.Result;
//        //    //foreach (var user in users)
//        //    //{
//        //    //    _allUsersCollection.Add(new AllUsersModel() { Username = user.Key, LoggedInTime = Math.Round(new TimeSpan(user.Sum).TotalHours, 1) });
//        //    //}

//        //}

//        //private void LoadBlockedApps()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    _blockedAppsCollection = new List<BlockedAppModel>();

//        //    var blockedApps = from b in App.DataAccess.Context.BlockedApps
//        //                      where b.UserID == Globals.SelectedUserID
//        //                      && b.Date >= Globals.Date1
//        //                      && b.Date <= Globals.Date2
//        //                      group b by b.Application.Name into g
//        //                      select new BlockedAppModel { AppName = g.Key, Count = g.Count() };
//        //    foreach (var blockedApp in blockedApps)
//        //    {
//        //        _blockedAppsCollection.Add(new BlockedAppModel() { AppName = blockedApp.AppName, Count = blockedApp.Count });
//        //    }
//        //}

//        //private void LoadDailyBlockedApps()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    _dailyBlockedAppCollection = new List<DailyBlockedAppModel>();

//        //    if (BlockedAppModel != null)
//        //    {
//        //        var blockedApps = from b in App.DataAccess.Context.BlockedApps
//        //                          where b.UserID == Globals.SelectedUserID
//        //                          && b.Application.Name == BlockedAppModel.AppName
//        //                          && b.Date >= Globals.Date1
//        //                          && b.Date <= Globals.Date2
//        //                          group b by new { Date = new DateTime(b.Date.Year, b.Date.Month, b.Date.Day) } into g
//        //                          select new { Date = g.Key.Date, Count = g.Count() };
//        //    }

//        //}

//        //private void LoadKeystrokes()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    // _keystrokesCollection = new List<KeystrokeModel>();
//        //    Task<List<KeystrokeModel>> task = Task.Factory.StartNew<List<KeystrokeModel>>(() =>
//        //    {
//        //        return (from u in App.DataAccess.UzerCollection
//        //                join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                where u.UserID == Globals.SelectedUserID
//        //                && l.DateCreated >= Globals.Date1
//        //                && l.DateCreated <= Globals.Date2
//        //                && a.Name != Constants.IDLE_APP_NAME
//        //                && l.HasKeystrokes
//        //                group l by l.AppName into g
//        //                select new KeystrokeModel { AppName = g.Key, Count = g.Sum(l => l.Keystrokes.Length) }).ToList();
//        //    });
//        //    _keystrokesCollection = task.Result;
//        //    //foreach (var log in logs)
//        //    //{
//        //    //    _keystrokesCollection.Add(new KeystrokeModel() { AppName = log.Key, Count = log.Count });
//        //    //}

//        //}

//        //private void LoadScreenshots()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_screenshotsCollection = new List<ScreenshotModel>();
//        //    Task<List<ScreenshotModel>> task = Task.Factory.StartNew<List<ScreenshotModel>>(() =>
//        //        {
//        //            return (from u in App.DataAccess.UzerCollection
//        //                    join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                    join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                    join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                    join s in App.DataAccess.ScreenshotCollection on l.LogID equals s.LogID
//        //                    where u.UserID == Globals.SelectedUserID
//        //                    && s.Date >= Globals.Date1
//        //                    && s.Date <= Globals.Date2
//        //                    && a.Name != Constants.IDLE_APP_NAME
//        //                    group s by s.AppName into g
//        //                    select new ScreenshotModel { AppName = g.Key, Count = g.Count() }).ToList();
//        //        });
//        //    _screenshotsCollection = task.Result;
//        //    //foreach (var screenshot in screenshots)
//        //    //{
//        //    //    _screenshotsCollection.Add(new ScreenshotModel() { AppName = screenshot.Key, Count = screenshot.Count });
//        //    //}
//        //}

//        //private void LoadUsage()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_usageCollection = new List<UsageModel>();
//        //    Task<List<UsageModel>> task = Task.Factory.StartNew<List<UsageModel>>(() =>
//        //    {
//        //        return (from l in App.DataAccess.LoginCollection
//        //                where l.UserID == Globals.SelectedUserID
//        //                && l.LoginDate >= Globals.Date1
//        //                && l.LoginDate <= Globals.Date2
//        //                group l by new { date = new DateTime(l.LoginDate.Year, l.LoginDate.Month, l.LoginDate.Day) } into g
//        //                select new UsageModel
//        //                {
//        //                    Date = g.Key.date.ToShortDateString(),
//        //                    Count = Math.Round(g.Sum(l => l.Duration.TotalHours), 1)
//        //                }).ToList();
//        //    });
//        //    _usageCollection = task.Result;
//        //    //var groupedLogins = from l in App.DataAccess.LoginCollection
//        //    //                    where l.UserID == Globals.SelectedUserID
//        //    //                    && l.LoginDate >= Globals.Date1
//        //    //                    && l.LoginDate <= Globals.Date2
//        //    //                    group l by new { date = new DateTime(l.LoginDate.Year, l.LoginDate.Month, l.LoginDate.Day) } into g
//        //    //                    select new UsageModel
//        //    //                    {
//        //    //                        Date = g.Key.date.ToShortDateString(),
//        //    //                        Count = Math.Round( g.Sum(l => l.Duration.TotalHours), 1)
//        //    //                    };
//        //    //foreach (var login in groupedLogins)
//        //    //{
//        //    //    _usageCollection.Add(new UsageModel() { Date = login.Date.ToShortDateString(), Count = Math.Round(login.Sum, 1) });
//        //    //}
//        //}

//        //private void LoadDailyApps()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    _dailyUsedAppsSeries = new List<DailyUsedAppsSeries>();

//        //    var dailyApps = from u in App.DataAccess.UzerCollection
//        //                    join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                    join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                    join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                    where u.UserID == Globals.SelectedUserID
//        //                    && l.DateCreated >= Globals.Date1
//        //                    && l.DateCreated <= Globals.Date2
//        //                    group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day), name = l.AppName } into g
//        //                    orderby g.Key.date
//        //                    select new
//        //                    {
//        //                        Date = g.Key.date,
//        //                        AppName = g.Key.name,
//        //                        Duration = g.Sum(l => l.Duration)
//        //                    };

//        //    ObservableCollection<MostUsedAppModel> dailyUsedAppsCollection;
//        //    foreach (var app in dailyApps)
//        //    {
//        //        if (!_dailyUsedAppsSeries.Exists(d => d.Date == app.Date.ToShortDateString()))
//        //        {
//        //            dailyUsedAppsCollection = new ObservableCollection<MostUsedAppModel>();
//        //            dailyUsedAppsCollection.Add(new Models.MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
//        //            _dailyUsedAppsSeries.Add(new Models.DailyUsedAppsSeries() { Date = app.Date.ToShortDateString(), DailyUsedAppsCollection = dailyUsedAppsCollection });
//        //        }
//        //        else
//        //        {
//        //            _dailyUsedAppsSeries.First(d => d.Date == app.Date.ToShortDateString()).DailyUsedAppsCollection.Add(new MostUsedAppModel() { AppName = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
//        //        }
//        //    }
//        //}


//        //private void LoadDailyAppUsage()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_dailyAppCollection = new List<DailyAppModel>();

//        //    if (MostUsedAppModel != null)
//        //    {
//        //        Task<List<DailyAppModel>> task = Task.Factory.StartNew<List<DailyAppModel>>(() =>
//        //        {
//        //            return (from a in App.DataAccess.ApplicationCollection
//        //                    join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                    join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                    where a.Name == MostUsedAppModel.AppName
//        //                    group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day) } into g
//        //                    orderby g.Key.date
//        //                    select new DailyAppModel { Date = g.Key.date.ToShortDateString(), Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) }).ToList();
//        //        });
//        //        _dailyAppCollection = task.Result;
//        //        //var groupedLogs = from a in App.DataAccess.ApplicationCollection
//        //        //                  join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //        //                  join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //        //                  where a.Name == MostUsedAppModel.AppName
//        //        //                  group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day) } into g
//        //        //                  orderby g.Key.date
//        //        //                  select new DailyAppModel { Date = g.Key.date.ToShortDateString(), Duration = Math.Round(new TimeSpan( g.Sum(l => l.Duration)).TotalHours,1) };
//        //        //foreach (var log in groupedLogs)
//        //        //{
//        //        //    _dailyAppCollection.Add(new DailyAppModel() { Date = log.Date.ToShortDateString(), Duration = Math.Round(new TimeSpan(log.Duration).TotalHours, 1) });
//        //        //}
//        //    }


//        //}

//        //private async Task<List<DailyAppModel>> LoadDailyAppUsageAsync()
//        //{
//        //    Task<List<DailyAppModel>> task = Task.Factory.StartNew<List<DailyAppModel>>(() =>
//        //    {
//        //        return (from a in App.DataAccess.ApplicationCollection
//        //                join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                where a.Name == MostUsedAppModel.AppName
//        //                group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day) } into g
//        //                orderby g.Key.date
//        //                select new DailyAppModel { Date = g.Key.date.ToShortDateString(), Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1) }).ToList();
//        //    });
//        //    return await task;
//        //}

//        //private void LoadDailyKeystrokes()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    _dailyKeystrokesCollection = new List<DailyKeystrokeModel>();

//        //    if (KeystrokeModel != null)
//        //    {
//        //        var logs = from u in App.DataAccess.UzerCollection
//        //                   join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                   join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                   join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                   where u.UserID == Globals.SelectedUserID
//        //                   && l.DateCreated >= Globals.Date1
//        //                   && l.DateCreated <= Globals.Date2
//        //                   && a.Name == KeystrokeModel.AppName
//        //                   group l by new { date = new DateTime(l.DateCreated.Year, l.DateCreated.Month, l.DateCreated.Day) } into g
//        //                   orderby g.Key.date
//        //                   select new { Date = g.Key.date, Count = g.Sum(l => l.Keystrokes.Length) };

//        //        foreach (var log in logs)
//        //        {
//        //            _dailyKeystrokesCollection.Add(new DailyKeystrokeModel() { Date = log.Date.ToShortDateString(), Count = log.Count });
//        //        }
//        //    }


//        //}

//        //private void LoadDailyScreenshots()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    _dailyScreenshotsCollection = new List<DailyScreenshotModel>();

//        //    if (ScreenshotModel != null)
//        //    {
//        //        var screenshots = from u in App.DataAccess.UzerCollection
//        //                          join a in App.DataAccess.ApplicationCollection on u.UserID equals a.UserID
//        //                          join w in App.DataAccess.WindowCollection on a.ApplicationID equals w.ApplicationID
//        //                          join l in App.DataAccess.LogCollection on w.WindowID equals l.WindowID
//        //                          join s in App.DataAccess.ScreenshotCollection on l.LogID equals s.LogID
//        //                          where u.UserID == Globals.SelectedUserID
//        //                          && s.Date >= Globals.Date1
//        //                          && s.Date <= Globals.Date2
//        //                          && a.Name != Constants.IDLE_APP_NAME
//        //                          group s by new { date = new DateTime(s.Date.Year, s.Date.Month, s.Date.Day) } into g
//        //                          orderby g.Key.date
//        //                          select new { Date = g.Key.date, Count = g.Count() };
//        //        foreach (var screenshot in screenshots)
//        //        {
//        //            _dailyScreenshotsCollection.Add(new DailyScreenshotModel() { Date = screenshot.Date.ToShortDateString(), Count = screenshot.Count });
//        //        }
//        //    }


//        //}


//        //private void LoadFilewatchers()
//        //{
//        //    DispatcherService.SetBusyState();
//        //    //_filewatcherCollection = new List<FilewatcherModel>();
//        //    Task<List<FilewatcherModel>> task = Task.Factory.StartNew<List<FilewatcherModel>>(() =>
//        //    {
//        //        return (from f in App.DataAccess.FileLogCollection
//        //                where f.UserID == Globals.SelectedUserID
//        //                && f.Date >= FirstDate
//        //                && f.Date <= LastDate
//        //                group f by f.Event into g
//        //                select new FilewatcherModel { Event = g.Key, Count = g.Count() }).ToList();
//        //    });
//        //    _filewatcherCollection = task.Result;
//        //    //_filewatcherCollection = (from f in App.DataAccess.FileLogCollection
//        //    //                          where f.UserID == Globals.SelectedUserID
//        //    //                          && f.Date >= FirstDate
//        //    //                          && f.Date <= LastDate
//        //    //                          group f by f.Event into g
//        //    //                          select new FilewatcherModel { Event = g.Key, Count = g.Count() }).ToList();


//        //    //foreach (var fileWatcher in filewatchers)
//        //    //{
//        //    //    _filewatcherCollection.Add(fileWatcher);
//        //    //}

//        //}

//        #endregion

//    }

//}

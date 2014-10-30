using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using AppsTracker.MVVM;
using System.Windows.Input;
using System.Collections.ObjectModel;
using AppsTracker.Utils;
using System.Diagnostics;
using AppsTracker.DAL;
using AppsTracker.DAL.Repos;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.ChartModels;

namespace AppsTracker.ViewModels
{
    internal sealed class Data_dayViewModel : ViewModelBase, IWorker, IChildVM, ICommunicator
    {
        #region Fields

        bool _working;

        DateTime _selectedDate = DateTime.Today;

        string _singleAppDuration;
        string _singleWindowDuration;
        string _duration;
        string _dayEnd;

        TopAppsModel _topAppsSingle;

        List<TopAppsModel> _topAppsList;
        List<DayViewModel> _dayViewModelList;
        List<TopWindowsModel> _topWindowsList;
        List<DailyUsageTypeSeries> _chartList;

        ICommand _singleAppSelectionChangedCommand;
        ICommand _singleWindowSelectionChangedCommand;
        ICommand _addDaysCommand;

        IRepository<Log> _logRepo;
        IRepository<Usage> _usageRepo;

        AppsEntities _context = new AppsEntities();

        #endregion

        #region Properties

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

        public string Title
        {
            get { return "DAY VIEW"; }
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
                LoadContent();
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
        public string Duration
        {
            get
            {
                return _duration;
            }
            set
            {
                _duration = value;
                PropertyChanging("Duration");
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
        public bool IsContentLoaded
        {
            get;
            private set;
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


        public List<TopAppsModel> TopAppsList
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
        public List<DayViewModel> DayViewModelList
        {
            get
            {
                return _dayViewModelList;
            }
            set
            {
                _dayViewModelList = value;
                PropertyChanging("DayViewModelList");
            }
        }
        public List<TopWindowsModel> TopWindowsList
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
        public List<DailyUsageTypeSeries> ChartList
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

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }
        #endregion

        public Data_dayViewModel()
        {
            _logRepo = RepositoryFactory.Instance.Get<IRepository<Log>>();
            _usageRepo = RepositoryFactory.Instance.Get<IRepository<Usage>>();
        }

        public async void LoadContent()
        {
            SingleAppDuration = string.Empty;
            SingleWindowDuration = string.Empty;
            Working = true;

            var daysTask = GetDayViewInfoAsync();
            var appsTask = GetTopAppsSingleAsync();

            var chartTask = GetChartContentAsync();
            var dayInfoTask = GetDayInfoAsync();

            await Task.WhenAll(daysTask, appsTask, chartTask, dayInfoTask);

            DayViewModelList = daysTask.Result;
            TopAppsList = appsTask.Result;
            ChartList = chartTask.Result;
            Duration = dayInfoTask.Result;

            Working = false;

            IsContentLoaded = true;
        }
        private async void LoadWindowsSingle()
        {
            Working = true;
            TopWindowsList = await GetTopWindowsSingleAsync().ConfigureAwait(false);
            Working = false;
        }

        private async Task<List<DayViewModel>> GetDayViewInfoAsync()
        {
            string ignore = UsageTypes.Login.ToString();
            DateTime date2 = _selectedDate.AddDays(1);

            var logs = _context.Logs.Where(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                      && l.DateCreated >= _selectedDate
                                                                      && l.DateCreated <= date2)
                                    .Include(l => l.Window.Application)
                                    .AsNoTracking()
                                    .ToList();

            var logsTask = LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.SelectedUserID
                                                                      && l.DateCreated >= _selectedDate
                                                                      && l.DateCreated <= date2
                                                                      , l => l.Window.Application);

            var usagesTask = UsageRepo.Instance.GetFilteredAsync(u => u.User.UserID == Globals.SelectedUserID
                                                                  && u.UsageStart >= _selectedDate
                                                                  && u.UsageEnd <= date2
                                                                  && u.UsageType.UType != ignore
                                                                  , u => u.UsageType);

            await Task.WhenAll(logsTask, usagesTask).ConfigureAwait(false);

            var logModels = logsTask.Result.Select(l => new DayViewModel()
                                                        {
                                                            DateCreated = l.DateCreated.ToString("HH:mm:ss"),
                                                            DateEnded = l.DateEnded.ToString("HH:mm:ss"),
                                                            Duration = l.Duration,
                                                            Name = l.Window.Application.Name,
                                                            Title = l.Window.Title
                                                        });

            var usageModels = usagesTask.Result.Select(u => new DayViewModel()
                                                           {
                                                               DateCreated = u.UsageStart.ToString("HH:mm:ss"),
                                                               DateEnded = u.UsageEnd.ToString("HH:mm:ss"),
                                                               Duration = u.Duration.Ticks,
                                                               Name = ((UsageTypes)Enum.Parse(typeof(UsageTypes), u.UsageType.UType)).ToExtendedString(),
                                                               Title = "*********",
                                                               IsRequested = true
                                                           });

            return logModels.Union(usageModels).OrderBy(d => d.DateCreated).ToList();

        }

        private async Task<List<TopAppsModel>> GetTopAppsSingleAsync()
        {
            DateTime date2 = _selectedDate.AddDays(1);
            IEnumerable<Log> logs = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.UserID
                                                                                        && l.DateCreated >= _selectedDate
                                                                                        && l.DateCreated <= date2
                                                                                        , l => l.Window.Application)
                                                                             .ConfigureAwait(false);
            double totalDuration = (from l in logs
                                    select (double?)l.Duration).Sum() ?? 0;

            var result = (from l in logs
                          group l by l.Window.Application.Name into grp
                          select grp).ToList()
                         .Select(g => new TopAppsModel { AppName = g.Key, Date = SelectedDate.ToShortDateString(), Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                         .OrderByDescending(t => t.Duration)
                         .ToList();

            var first = result.FirstOrDefault();
            if (first != null)
                first.IsSelected = true;

            return result;
        }

        private async Task<List<TopWindowsModel>> GetTopWindowsSingleAsync()
        {
            if (TopAppsSingle == null)
                return null;

            string appName = TopAppsSingle.AppName;
            var nextDay = _selectedDate.AddDays(1);

            var total = await LogRepo.Instance.GetFilteredAsync(l => l.Window.Application.User.UserID == Globals.UserID
                                                         && l.DateCreated >= _selectedDate
                                                         && l.DateCreated <= nextDay
                                                         && l.Window.Application.Name == appName
                                                         , l => l.Window)
                                                     .ConfigureAwait(false);

            double totalDuration = total.Sum(l => l.Duration);

            return total.GroupBy(l => l.Window.Title)
                                  .Select(g => new TopWindowsModel { Title = g.Key, Usage = (g.Sum(l => l.Duration) / totalDuration), Duration = g.Sum(l => l.Duration) })
                                  .OrderByDescending(t => t.Duration)
                                  .ToList();
        }

        private async Task<string> GetDayInfoAsync()
        {
            DateTime today = SelectedDate.Date;
            DateTime nextDay = today.AddDays(1d);
            string loginType = UsageTypes.Login.ToString();

            var logins = await UsageRepo.Instance.GetFilteredAsync(u => u.User.UserID == Globals.UserID
                                                                    && u.UsageStart >= today
                                                                    && u.UsageStart <= nextDay
                                                                    && u.UsageType.UType == loginType)
                                                   .ConfigureAwait(false);

            var loginBegin = logins.OrderBy(l => l.UsageStart).FirstOrDefault();

            var loginEnd = logins.OrderByDescending(l => l.UsageEnd).FirstOrDefault();

            var totalDuraion = new TimeSpan(logins.Sum(l => l.Duration.Ticks));

            string dayBegin = loginBegin == null ? "N/A" : loginBegin.UsageStart.ToShortTimeString();
            string dayEnd = (loginEnd == null || loginEnd.IsCurrent) ? "N/A" : loginEnd.UsageEnd.ToShortTimeString();
            string totalHours = totalDuraion.ToString(@"hh\:mm");

            return string.Format("Day start: {0}   -   Day end: {1} \t\t Total duration: {2}", dayBegin, dayEnd, totalHours);
        }

        private Task<List<DailyUsageTypeSeries>> GetChartContentAsync()
        {
            return Task<List<DailyUsageTypeSeries>>.Run(async () =>
            {
                DateTime today = SelectedDate.Date;
                DateTime nextDay = today.AddDays(1d);

                string usageLogin = UsageTypes.Login.ToString();
                string usageIdle = UsageTypes.Idle.ToString();
                string usageLocked = UsageTypes.Locked.ToString();
                string usageStopped = UsageTypes.Stopped.ToString();

                List<Usage> logins;
                List<Usage> idles;
                List<Usage> lockeds;
                List<Usage> stoppeds;

                logins = (await UsageRepo.Instance.GetFilteredAsync(u => u.User.UserID == Globals.UserID
                                                                    && u.UsageStart >= today
                                                                    && u.UsageStart <= nextDay
                                                                    && u.UsageType.UType == usageLogin
                                                                    , u => u.UsageType)
                                                    .ConfigureAwait(false))
                                                    .ToList();

                var usageIDs = logins.Select(u => u.UsageID);

                var idlesTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                    && usageIDs.Contains(u.SelfUsageID.Value)
                                                                    && u.UsageType.UType == usageIdle
                                                                    , u => u.UsageType);

                var lockedsTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                        && usageIDs.Contains(u.SelfUsageID.Value)
                                                                        && u.UsageType.UType == usageLocked
                                                                        , u => u.UsageType);

                var stoppedsTask = UsageRepo.Instance.GetFilteredAsync(u => u.SelfUsageID.HasValue
                                                                         && usageIDs.Contains(u.SelfUsageID.Value)
                                                                         && u.UsageType.UType == usageStopped
                                                                         , u => u.UsageType);

                await Task.WhenAll(idlesTask, lockedsTask, stoppedsTask);

                idles = idlesTask.Result.ToList();
                lockeds = lockedsTask.Result.ToList();
                stoppeds = stoppedsTask.Result.ToList();

                List<DailyUsageTypeSeries> collection = new List<DailyUsageTypeSeries>();

                foreach (var login in logins)
                {
                    DailyUsageTypeSeries series = new DailyUsageTypeSeries() { Time = login.UsageStart.ToString("HH:mm:ss") };
                    ObservableCollection<UsageTypeModel> observableCollection = new ObservableCollection<UsageTypeModel>();

                    long idleTime = 0;
                    long lockedTime = 0;
                    long loginTime = 0;
                    long stoppedTime = 0;

                    var currentIdles = idles.Where(u => u.SelfUsageID == login.UsageID);
                    var currentLockeds = lockeds.Where(u => u.SelfUsageID == login.UsageID);
                    var currentStopppeds = stoppeds.Where(u => u.SelfUsageID == login.UsageID);

                    if (currentIdles.Count() > 0)
                    {
                        idleTime = currentIdles.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTime).TotalHours, 2), UsageType = usageIdle });
                    }

                    if (currentLockeds.Count() > 0)
                    {
                        lockedTime = currentLockeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Computer locked" });
                    }


                    if (currentStopppeds.Count() > 0)
                    {
                        stoppedTime = currentStopppeds.Sum(l => l.Duration.Ticks);
                        observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTime).TotalHours, 2), UsageType = "Stopped logging" });
                    }

                    loginTime = login.Duration.Ticks - lockedTime - idleTime - stoppedTime;
                    observableCollection.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTime).TotalHours, 2), UsageType = "Work" });


                    series.DailyUsageTypeCollection = observableCollection;

                    collection.Add(series);
                }

                if (logins.Count > 1)
                {
                    DailyUsageTypeSeries seriesTotal = new DailyUsageTypeSeries() { Time = "TOTAL" };
                    ObservableCollection<UsageTypeModel> observableCollectionTotal = new ObservableCollection<UsageTypeModel>();

                    long idleTimeTotal = 0;
                    long lockedTimeTotal = 0;
                    long loginTimeTotal = 0;
                    long stoppedTimeTotal = 0;

                    if (idles.Count > 0)
                    {
                        idleTimeTotal = idles.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(idleTimeTotal).TotalHours, 2), UsageType = usageIdle });
                    }

                    if (lockeds.Count > 0)
                    {
                        lockedTimeTotal = lockeds.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(lockedTimeTotal).TotalHours, 2), UsageType = "Computer locked" });
                    }

                    if (logins.Count > 0)
                    {
                        loginTimeTotal = logins.Sum(l => l.Duration.Ticks) - lockedTimeTotal - idleTimeTotal;
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(loginTimeTotal).TotalHours, 2), UsageType = "Work" });
                    }

                    if (stoppeds.Count > 0)
                    {
                        stoppedTimeTotal = stoppeds.Sum(l => l.Duration.Ticks);
                        observableCollectionTotal.Add(new UsageTypeModel() { Time = Math.Round(new TimeSpan(stoppedTimeTotal).TotalHours, 2), UsageType = "Stopped logging" });
                    }

                    seriesTotal.DailyUsageTypeCollection = observableCollectionTotal;

                    collection.Add(seriesTotal);
                }

                return collection;
            });
        }

        #region Commmand Methods

        private void SingleAppSelectionChanged()
        {
            List<TopAppsModel> topAppCollection = TopAppsList;
            if (topAppCollection == null)
                return;
            long ticks = 0;
            var selected = topAppCollection.Where(t => t.IsSelected);
            foreach (var app in selected)
                ticks += app.Duration;
            if (ticks == 0)
                return;
            TimeSpan timeSpan = new TimeSpan(ticks);
            SingleAppDuration = string.Format("Selected: {0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }
        private void SingleWindowSelectionChanged()
        {
            List<TopWindowsModel> topWindowCollection = TopWindowsList;
            if (topWindowCollection == null)
                return;
            long ticks = 0;
            var selected = topWindowCollection.Where(t => t.IsSelected);
            foreach (var window in selected)
                ticks += window.Duration;
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

        protected override void Disposing()
        {
            _context.Dispose();
            base.Disposing();
        }
    }
}

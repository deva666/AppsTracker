using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Hooks;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(IModule))]
    internal sealed class LimitObserver : IModule
    {
        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IWindowNotifier windowNotifier;
        private readonly IMidnightNotifier midnightNotifier;
        private readonly ILimitHandler limitHandler;
        private readonly IMediator mediator;

        private readonly IDictionary<string, IEnumerable<AppLimit>> appLimitsMap = new Dictionary<string, IEnumerable<AppLimit>>();

        private readonly Timer dayTimer;
        private readonly Timer weekTimer;


        private string currentApp;
        private AppLimit currentLimit;


        [ImportingConstructor]
        public LimitObserver(ITrackingService trackingService,
                             IDataService dataService,
                             IWindowNotifier windowNotifier,
                             IMidnightNotifier midnightNotifier,
                             ILimitHandler limitHandler,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.windowNotifier = windowNotifier;
            this.midnightNotifier = midnightNotifier;
            this.limitHandler = limitHandler;
            this.mediator = mediator;

            dayTimer = new Timer(TimerCallback,
                new Func<AppLimit>(() => Volatile.Read(ref currentLimit)), Timeout.Infinite, Timeout.Infinite);
            weekTimer = new Timer(TimerCallback);

            mediator.Register(MediatorMessages.APP_LIMITS_CHANGIING, LoadAppLimits);
            mediator.Register(MediatorMessages.STOP_TRACKING, StopTimers);
            mediator.Register(MediatorMessages.RESUME_TRACKING, CheckLimits);
        }

        private void TimerCallback(object state)
        {
            var valueFactory = (Func<AppLimit>)state;
            limitHandler.Handle(valueFactory.Invoke());
        }

        public void SettingsChanged(Setting settings)
        {
        }

        public void Initialize(Setting settings)
        {
            midnightNotifier.MidnightTick += MidnightTick;
            windowNotifier.WindowChanged += windowNotifier_WindowChanged;

            LoadAppLimits();
        }


        private void MidnightTick(object sender, EventArgs e)
        {
            StopTimers();
            CheckLimits();
        }

        private void CheckLimits()
        {
            var limit = Volatile.Read(ref currentLimit);
            if (limit != null && limit.Application != null)
                LoadAppDurations(limit.Application);
        }

        private void windowNotifier_WindowChanged(object sender, WindowChangedArgs e)
        {
            var app = trackingService.GetApp(e.AppInfo);
            LoadAppDurations(app);
        }


        private void LoadAppLimits()
        {
            var appsWithLimits = dataService.GetFiltered<Aplication>(a => a.Limits.Count > 0
                                                                     && a.UserID == trackingService.UserID,
                                                                     a => a.Limits);
            appLimitsMap.Clear();

            foreach (var app in appsWithLimits)
            {
                appLimitsMap.Add(app.Name, app.Limits);
            }
        }

        private void LoadAppDurations(Aplication app)
        {
            if (app == null)
            {
                currentApp = string.Empty;
                return;
            }

            currentApp = app.Name;
            StopTimers();
            if (appLimitsMap.ContainsKey(app.Name))
            {
                var dayDurationTask = GetDayAppDuration(app);
                dayDurationTask.ContinueWith(GetAppDurationContinuation, CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void GetAppDurationContinuation(Task<Tuple<Aplication, long>> task)
        {
            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(task.Result.Item1.Name, out limits) == false)
                return;

            var limit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Day);
            if (limit == null)
                return;

            if (task.Result.Item2 >= limit.Limit)
            {
                limitHandler.Handle(limit);
            }
            else if (currentApp != task.Result.Item1.Name)
            {
                return;
            }
            else
            {
                Volatile.Write(ref currentLimit, limit);
                dayTimer.Change(new TimeSpan((limit.Limit - task.Result.Item2)), Timeout.InfiniteTimeSpan);
            }
        }

        private void StopTimers()
        {
            dayTimer.Change(Timeout.Infinite, Timeout.Infinite);
            weekTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task<Tuple<Aplication, long>> GetDayAppDuration(Aplication app)
        {
            var duration = await GetDayDurationAsync(app);
            return new Tuple<Aplication, long>(app, duration);
        }

        private Task<long> GetDayDurationAsync(Aplication app)
        {
            return Task.Run(() => GetDayDuration(app));
        }

        private long GetDayDuration(Aplication app)
        {
            var loadedApp = dataService.GetFiltered<Aplication>(a => a.ApplicationID == app.ApplicationID,
                                                                a => a.Windows.Select(w => w.Logs))
                                                                .First();
            return loadedApp.Windows.SelectMany(w => w.Logs)
                                    .Where(l => l.DateCreated >= DateTime.Now.Date)
                                    .Sum(l => l.Duration);
        }

        private Task<long> GetWeekDurationAsync(Aplication app)
        {
            return Task.Run(() => GetWeekDuration(app));
        }

        private long GetWeekDuration(Aplication app)
        {
            DateTime now = DateTime.Today;
            int delta = DayOfWeek.Monday - now.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            DateTime weekBegin = now.AddDays(delta);
            DateTime weekEnd = weekBegin.AddDays(6);

            var loadedApp = dataService.GetFiltered<Aplication>(a => a.ApplicationID == app.ApplicationID,
                                                                a => a.Windows.Select(w => w.Logs))
                                                                .First();
            return loadedApp.Windows
                            .SelectMany(w => w.Logs)
                            .Where(l => l.DateCreated >= weekBegin && l.DateCreated <= weekEnd)
                            .Sum(l => l.Duration);
        }


        public void Dispose()
        {
            dayTimer.Dispose();
            weekTimer.Dispose();
        }


        public int InitializationOrder
        {
            get { return 3; }
        }
    }
}

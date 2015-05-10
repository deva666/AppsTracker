using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    internal sealed class LimitObserver
    {
        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IMidnightNotifier midnightNotifier;
        private readonly ILimitHandler limitHandler;
        private readonly IMediator mediator;

        private readonly Timer dayTimer;
        private readonly Timer weekTimer;
        private readonly Timer monthTimer;

        private readonly Dictionary<Aplication, IEnumerable<AppLimit>> appLimitsMap = new Dictionary<Aplication, IEnumerable<AppLimit>>();

        private Aplication currentApp;
        private AppLimit currentWarning;


        [ImportingConstructor]
        public LimitObserver(IMidnightNotifier midnightNotifier,
                             ILimitHandler limitHandler,
                             IDataService dataService,
                             ITrackingService trackingService,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.midnightNotifier = midnightNotifier;
            this.limitHandler = limitHandler;
            this.mediator = mediator;

            Initialize();

            dayTimer = new Timer(TimerCallback, currentWarning, Timeout.Infinite, Timeout.Infinite);
            weekTimer = new Timer(TimerCallback);
            monthTimer = new Timer(TimerCallback);
        }


        private void Initialize()
        {
            midnightNotifier.MidnightTick += MidnightTick;

            var appsWithLimits = dataService.GetFiltered<Aplication>(a => a.Limits.Count > 0
                                                                     && a.UserID == trackingService.UserID,
                                                                     a => a.Limits);
            foreach (var app in appsWithLimits)
            {
                appLimitsMap.Add(app, app.Limits);
            }
        }

        private void MidnightTick(object sender, EventArgs e)
        {
            //stop timers, load current app duration
        }

        private void TimerCallback(object state)
        {
            var warning = (AppLimit)state;
            limitHandler.Handle(warning);
        }


        public void AppChanged(Aplication app)
        {
            currentApp = app;
            StopTimers();
            if (appLimitsMap.ContainsKey(app))
            {
                GetAppDuration(app).ContinueWith(GetAppDurationContinuation,
                    SynchronizationContext.Current, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private void GetAppDurationContinuation(Task<Tuple<Aplication, long>> task, object state)
        {
            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(task.Result.Item1, out limits) == false)
                return;

            var limit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Day);
            if (limit == null)
                return;

            if (task.Result.Item2 >= limit.Limit)
            {
                limitHandler.Handle(limit);
            }
            else if (currentApp != task.Result.Item1)
            {
                return;
            }
            else
            {
                currentWarning = limit;
                dayTimer.Change(new TimeSpan((limit.Limit - task.Result.Item2)), Timeout.InfiniteTimeSpan);
            }
        }

        private void StopTimers()
        {
            dayTimer.Change(Timeout.Infinite, Timeout.Infinite);
            weekTimer.Change(Timeout.Infinite, Timeout.Infinite);
            monthTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task<Tuple<Aplication, long>> GetAppDuration(Aplication app)
        {
            var duration = await GetTodayDurationAsync(app);
            return new Tuple<Aplication, long>(app, duration);
        }

        private Task<long> GetTodayDurationAsync(Aplication app)
        {
            return Task<long>.Run(() => GetTodayDuration(app));
        }

        private long GetTodayDuration(Aplication app)
        {
            var loadedApp = dataService.GetFiltered<Aplication>(a => a.UserID == trackingService.UserID
                                                                && a.ApplicationID == app.ApplicationID,
                                                                a => a.Windows.Select(w => w.Logs))
                                                                .First();
            return loadedApp.Windows.SelectMany(w => w.Logs)
                                    .Where(l => l.DateCreated >= DateTime.Now.Date)
                                    .Sum(l => l.Duration);
        }

        private long GetWeekDuration(Aplication app)
        {
            DateTime now = DateTime.Today;
            int delta = DayOfWeek.Monday - now.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            DateTime weekBegin = now.AddDays(delta);
            DateTime weekEnd = weekBegin.AddDays(6);

            using (var context = new AppsEntities())
            {
                var loadedApp = context.Applications.Include(a => a.Windows.Select(w => w.Logs)).First(a => a.ApplicationID == app.ApplicationID);
                return loadedApp.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= weekBegin && l.DateCreated <= weekEnd).Sum(l => l.Duration);
            }
        }

        private long GetMonthDuration(Aplication app)
        {
            DateTime now = DateTime.Now;
            var monthBegin = new DateTime(now.Year, now.Month, 1);
            int lastDay = DateTime.DaysInMonth(now.Year, now.Month);
            var monthEnd = new DateTime(now.Year, now.Month, lastDay);

            using (var context = new AppsEntities())
            {
                var loadedApp = context.Applications.Include(a => a.Windows.Select(w => w.Logs)).First(a => a.ApplicationID == app.ApplicationID);
                return loadedApp.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= monthBegin && l.DateCreated <= monthEnd).Sum(l => l.Duration);
            }
        }
    }
}

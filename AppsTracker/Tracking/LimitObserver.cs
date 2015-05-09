using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    internal sealed class LimitObserver
    {
        private readonly IMidnightNotifier midnightNotifier;
        private readonly ILimitHandler limitHandler;

        private readonly Timer dayTimer;
        private readonly Timer weekTimer;
        private readonly Timer monthTimer;

        private readonly Dictionary<Aplication, AppWarning> appLimitsMap = new Dictionary<Aplication, AppWarning>();

        private Aplication currentApp;
        private AppWarning currentWarning;


        [ImportingConstructor]
        public LimitObserver(IMidnightNotifier midnightNotifier, ILimitHandler limitHandler)
        {
            this.midnightNotifier = midnightNotifier;
            this.midnightNotifier.MidnightTick += MidnightTick;
            this.limitHandler = limitHandler;

            dayTimer = new Timer(DayTimerCallback, currentWarning, Timeout.Infinite, Timeout.Infinite);
            weekTimer = new Timer(WeekTimerCallback);
            monthTimer = new Timer(MonthTimerCallback);
        }


        private void Initialize()
        {
            using (var context = new AppsEntities())
            {
                var appsWithLimits = context.AppWarnings;
                foreach (var appWarning in appsWithLimits)
                {
                    appLimitsMap.Add(appWarning.Application, appWarning);
                }

            }
        }

        private void MidnightTick(object sender, EventArgs e)
        {
            //stop timers, load current app duration
        }

        private void DayTimerCallback(object state)
        {
            var warning = (AppWarning)state;
            limitHandler.Handle(warning);
        }

        private void WeekTimerCallback(object state)
        {

        }

        private void MonthTimerCallback(object state)
        {

        }

        public void AppChanged(Aplication app)
        {
            currentApp = app;
            StopTimers();
            if (appLimitsMap.ContainsKey(app))
            {
                GetAppDuration(app).ContinueWith(GetAppDurationContinuation, SynchronizationContext.Current, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private void GetAppDurationContinuation(Task<Tuple<Aplication, long>> task, object state)
        {
            AppWarning warning;
            if (appLimitsMap.TryGetValue(task.Result.Item1, out warning) == false)
                return;

            if (task.Result.Item2 >= warning.Limit)
            {
                limitHandler.Handle(warning);
            }
            else if (currentApp != task.Result.Item1)
                return;
            else
            {
                currentWarning = warning;
                dayTimer.Change(new TimeSpan((warning.Limit - task.Result.Item2)), Timeout.InfiniteTimeSpan);
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

        private async Task SetupDayWarning(AppWarning dayWarning)
        {
            if (dayWarning == null)
                return;

            var duration = await GetTodayDurationAsync(dayWarning.Application);
            if (duration > dayWarning.Limit)
            {
                DayTimerCallback(null);
                return;
            }

            var ticksTillMidnight = DateTime.Now.AddDays(1).Date.Ticks - DateTime.Now.Ticks;
            var ticksTillLimitReached = dayWarning.Limit - duration;
            if (ticksTillMidnight < ticksTillLimitReached)
                return;

            dayTimer.Change(new TimeSpan(ticksTillLimitReached), Timeout.InfiniteTimeSpan);
        }

        private Task<long> GetTodayDurationAsync(Aplication app)
        {
            return Task<long>.Run(() => GetTodayDuration(app));
        }

        private long GetTodayDuration(Aplication app)
        {
            using (var context = new AppsEntities())
            {
                var loadedApp = context.Applications.Include(a => a.Windows.Select(w => w.Logs)).First(a => a.ApplicationID == app.ApplicationID);
                return loadedApp.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= DateTime.Now.Date).Sum(l => l.Duration);
            }
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

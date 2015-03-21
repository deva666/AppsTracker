using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using System.Threading;

namespace AppsTracker.Logging
{
    internal sealed class AppWatcher
    {
        private readonly Timer dayTimer;
        private readonly Timer weekTimer;
        private readonly Timer monthTimer;

        private AppWarning dayWarning;
        private AppWarning weekWarning;
        private AppWarning monthWarning;

        public AppWatcher()
        {
            dayTimer = new Timer(DayTimerCallback);
            weekTimer = new Timer(WeekTimerCallback);
            monthTimer = new Timer(MonthTimerCallback);
        }

        private void DayTimerCallback(object state)
        {
            switch (dayWarning.TimeElapsedAction)
            {
                case TimeElapsedAction.Warn:
                    break;
                case TimeElapsedAction.Shutdown:
                    break;
                case TimeElapsedAction.None:
                    break;
            }
        }

        private void WeekTimerCallback(object state)
        {

        }

        private void MonthTimerCallback(object state)
        {

        }

        public void AppChanged(Aplication app)
        {
            StopTimers();

            using (var context = new AppsEntities())
            {
                var warnings = context.AppWarnings.Include(w => w.Application).Where(w => w.Application.ApplicationID == app.ApplicationID);
                dayWarning = warnings.FirstOrDefault(w => w.WarningSpan == WarningSpan.Day);
                var daySetupTask = SetupDayWarning(dayWarning);
            }
        }

        private void StopTimers()
        {
            dayTimer.Change(Timeout.Infinite, Timeout.Infinite);
            weekTimer.Change(Timeout.Infinite, Timeout.Infinite);
            monthTimer.Change(Timeout.Infinite, Timeout.Infinite);
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

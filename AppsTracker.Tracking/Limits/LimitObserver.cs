using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Hooks;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class LimitObserver : ITrackingModule
    {
        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IAppChangedNotifier appChangedNotifier;
        private readonly IMidnightNotifier midnightNotifier;
        private readonly ILimitHandler limitHandler;
        private readonly IMediator mediator;

        private readonly IDictionary<Aplication, IEnumerable<AppLimit>> appLimitsMap = new Dictionary<Aplication, IEnumerable<AppLimit>>();

        private readonly Timer dayTimer;
        private readonly Timer weekTimer;

        private AppLimit currentDayLimit;
        private AppLimit currentWeekLimit;

        private Int32 activeAppId;
        private AppInfo activeAppInfo;

        [ImportingConstructor]
        public LimitObserver(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IMidnightNotifier midnightNotifier,
                             ILimitHandler limitHandler,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appChangedNotifier = appChangedNotifier;
            this.midnightNotifier = midnightNotifier;
            this.limitHandler = limitHandler;
            this.mediator = mediator;

            dayTimer = new Timer(TimerCallback,
                new Func<AppLimit>(() => Volatile.Read(ref currentDayLimit)), Timeout.Infinite, Timeout.Infinite);
            weekTimer = new Timer(TimerCallback,
                new Func<AppLimit>(() => Volatile.Read(ref currentWeekLimit)), Timeout.Infinite, Timeout.Infinite);

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
            midnightNotifier.MidnightTick += OnMidnightTick;
            appChangedNotifier.AppChanged += OnAppChanged;

            LoadAppLimits();
        }

        private void LoadAppLimits()
        {
            var appsWithLimits = dataService.GetFiltered<Aplication>(a => a.Limits.Count > 0
                                                                     && a.UserID == trackingService.UserID,
                                                                     a => a.Limits);
            appLimitsMap.Clear();

            foreach (var app in appsWithLimits)
            {
                appLimitsMap.Add(app, app.Limits);
            }
        }

        private void OnMidnightTick(object sender, EventArgs e)
        {
            StopTimers();
            CheckLimits();
        }

        private void StopTimers()
        {
            dayTimer.Change(Timeout.Infinite, Timeout.Infinite);
            weekTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }


        private void CheckLimits()
        {
            var dayLimit = Volatile.Read(ref currentDayLimit);
            var weekLimit = Volatile.Read(ref currentWeekLimit);
            if (dayLimit != null && dayLimit.Application != null)
                LoadAppDurations(dayLimit.Application);
            else if (weekLimit != null && weekLimit.Application != null)
                LoadAppDurations(weekLimit.Application);
        }


        private async void OnAppChanged(object sender, AppChangedArgs e)
        {
            if (activeAppInfo == e.LogInfo.AppInfo)
                return;

            activeAppInfo = e.LogInfo.AppInfo;
            StopTimers();
            currentDayLimit = currentWeekLimit = null;
            var app = await trackingService.GetAppAsync(e.LogInfo.AppInfo);
            if (app != null && app.AppInfo == activeAppInfo)
                LoadAppDurations(app);
        }


        private void LoadAppDurations(Aplication app)
        {
            if (app == null)
            {
                activeAppId = -1;
                return;
            }

            activeAppId = app.ApplicationID;
            if (appLimitsMap.ContainsKey(app))
            {
                var limits = appLimitsMap[app];
                var dailyLimit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Day);
                var weeklyLimit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Week);

                var scheduler = TaskScheduler.Current;

                if (dailyLimit != null)
                {
                    var dayDurationTask = GetDayDurationAsync(app);
                    dayDurationTask.ContinueWith(GetAppDurationContinuation, dailyLimit, CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
                }
                if (weeklyLimit != null)
                {
                    var weekDurationTask = GetWeekDurationAsync(app);
                    weekDurationTask.ContinueWith(GetAppDurationContinuation, weeklyLimit, CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion, scheduler);
                }
            }
        }

        private void GetAppDurationContinuation(Task<long> task, object state)
        {
            var appLimit = (AppLimit)state;

            if (task.Result >= appLimit.Limit)
            {
                limitHandler.Handle(appLimit);
            }
            else if (activeAppId != appLimit.ApplicationID)
            {
                return;
            }
            else
            {
                switch (appLimit.LimitSpan)
                {
                    case LimitSpan.Day:
                        Volatile.Write(ref currentDayLimit, appLimit);
                        dayTimer.Change(new TimeSpan((appLimit.Limit - task.Result)), Timeout.InfiniteTimeSpan);
                        break;
                    case LimitSpan.Week:
                        Volatile.Write(ref currentWeekLimit, appLimit);
                        weekTimer.Change(new TimeSpan((appLimit.Limit - task.Result)), Timeout.InfiniteTimeSpan);
                        break;
                }
            }
        }


        private Task<long> GetDayDurationAsync(Aplication app)
        {
            return Task.Run(() => trackingService.GetDayDuration(app));
        }


        private Task<long> GetWeekDurationAsync(Aplication app)
        {
            return Task.Run(() => trackingService.GetWeekDuration(app));
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

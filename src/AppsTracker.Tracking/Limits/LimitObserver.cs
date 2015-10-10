using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Common.Utils;
using AppsTracker.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Tracking.Hooks;

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
        private readonly IWorkQueue workQueue;
        private readonly ISyncContext syncContext;

        private readonly IDictionary<Aplication, IEnumerable<AppLimit>> appLimitsMap
            = new Dictionary<Aplication, IEnumerable<AppLimit>>();

        private readonly Timer dayTimer;
        private readonly Timer weekTimer;

        private AppLimit currentDayLimit;
        private AppLimit currentWeekLimit;

        private Int32 activeAppId;
        private AppInfo activeAppInfo;
        private String activeWindowTitle;

        [ImportingConstructor]
        public LimitObserver(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IMidnightNotifier midnightNotifier,
                             ILimitHandler limitHandler,
                             IMediator mediator,
                             IWorkQueue workQueue,
                             ISyncContext syncContext)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appChangedNotifier = appChangedNotifier;
            this.midnightNotifier = midnightNotifier;
            this.limitHandler = limitHandler;
            this.mediator = mediator;
            this.workQueue = workQueue;
            this.syncContext = syncContext;

            dayTimer = new Timer(TimerCallback,
                new Func<AppLimit>(() => currentDayLimit), Timeout.Infinite, Timeout.Infinite);
            weekTimer = new Timer(TimerCallback,
                new Func<AppLimit>(() => currentWeekLimit), Timeout.Infinite, Timeout.Infinite);

            mediator.Register(MediatorMessages.APP_LIMITS_CHANGIING, LoadAppLimits);
            mediator.Register(MediatorMessages.STOP_TRACKING, StopTimers);
            mediator.Register(MediatorMessages.RESUME_TRACKING, CheckLimits);
        }

        private void TimerCallback(object state)
        {
            syncContext.Invoke(() =>
            {
                var valueFactory = (Func<AppLimit>)state;
                var limit = valueFactory.Invoke();
                if (limit != null)
                    limitHandler.Handle(limit);
            });
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
            if (currentDayLimit != null && currentDayLimit.Application != null)
                LoadAppDurations(currentDayLimit.Application);
            else if (currentWeekLimit != null && currentWeekLimit.Application != null)
                LoadAppDurations(currentWeekLimit.Application);
        }


        private async void OnAppChanged(object sender, AppChangedArgs e)
        {
            if ((activeAppInfo == e.LogInfo.AppInfo && activeWindowTitle == e.LogInfo.WindowTitle)
                || appLimitsMap.Count == 0)
                return;

            activeAppInfo = e.LogInfo.AppInfo;
            activeWindowTitle = e.LogInfo.WindowTitle;
            StopTimers();
            currentDayLimit = currentWeekLimit = null;
            var valueFactory = new Func<Object>(() => trackingService.GetApp(e.LogInfo.AppInfo));
            var app = (Aplication)await workQueue.EnqueueWork(valueFactory);
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
            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(app, out limits))
            {
                var dailyLimit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Day);
                var weeklyLimit = limits.FirstOrDefault(l => l.LimitSpan == LimitSpan.Week);

                if (dailyLimit != null)
                {
                    var dayDurationTask = workQueue.EnqueueWork(() => trackingService.GetDayDuration(app));
                    dayDurationTask.ContinueWith(OnGetAppDuration, dailyLimit, CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
                }
                if (weeklyLimit != null)
                {
                    var weekDurationTask = workQueue.EnqueueWork(() => trackingService.GetWeekDuration(app));
                    weekDurationTask.ContinueWith(OnGetAppDuration, weeklyLimit, CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        private void OnGetAppDuration(Task<Object> task, object state)
        {
            var appLimit = (AppLimit)state;
            var duration = (Int64)task.Result;

            if (duration >= appLimit.Limit)
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
                        currentDayLimit = appLimit;
                        dayTimer.Change(new TimeSpan((appLimit.Limit - duration)), Timeout.InfiniteTimeSpan);
                        break;
                    case LimitSpan.Week:
                        currentWeekLimit = appLimit;
                        weekTimer.Change(new TimeSpan((appLimit.Limit - duration)), Timeout.InfiniteTimeSpan);
                        break;
                }
            }
        }

        public void Dispose()
        {
            dayTimer.Dispose();
            weekTimer.Dispose();
            midnightNotifier.Dispose();
        }


        public int InitializationOrder
        {
            get { return 3; }
        }
    }
}

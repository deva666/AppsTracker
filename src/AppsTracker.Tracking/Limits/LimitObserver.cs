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
using AppsTracker.Tracking.Hooks;

namespace AppsTracker.Tracking.Limits
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

        private readonly IDictionary<Aplication, IEnumerable<AppLimit>> appLimitsMap
            = new Dictionary<Aplication, IEnumerable<AppLimit>>();

        private readonly ICollection<LimitNotifier> limitNotifiers
            = new List<LimitNotifier>();

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

            limitNotifiers.Add(new LimitNotifier(syncContext, LimitSpan.Day));
            limitNotifiers.Add(new LimitNotifier(syncContext, LimitSpan.Week));

            foreach (var notifier in limitNotifiers)
            {
                notifier.LimitReached += OnLimitReached;
            }
            
            mediator.Register(MediatorMessages.APP_LIMITS_CHANGIING, LoadAppLimits);
            mediator.Register(MediatorMessages.STOP_TRACKING, StopTimers);
            mediator.Register(MediatorMessages.RESUME_TRACKING, CheckLimits);
        }

        private void OnLimitReached(object sender, LimitReachedArgs args)
        {
            if (args.Limit != null)
                limitHandler.Handle(args.Limit);
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
            foreach (var notifer in limitNotifiers)
            {
                notifer.StopTimer();
            }
        }


        private void CheckLimits()
        {
            foreach (var notifier in limitNotifiers.Where(l => l.Limit != null))
            {
                LoadAppDurations(notifier.Limit.Application);
            }
        }


        private async void OnAppChanged(object sender, AppChangedArgs e)
        {
            if ((activeAppInfo == e.LogInfo.AppInfo && activeWindowTitle == e.LogInfo.WindowTitle)
                || appLimitsMap.Count == 0)
                return;

            activeAppInfo = e.LogInfo.AppInfo;
            activeWindowTitle = e.LogInfo.WindowTitle;
            
            StopTimers();
            ClearLimits();

            var valueFactory = new Func<Object>(() => trackingService.GetApp(e.LogInfo.AppInfo));
            var app = (Aplication)await workQueue.EnqueueWork(valueFactory);
            if (app != null && app.AppInfo == activeAppInfo)
                LoadAppDurations(app);
        }

        private void ClearLimits()
        {
            foreach (var notifier in limitNotifiers)
            {
                notifier.Limit = null;
            }
        }


        private void LoadAppDurations(Aplication app)
        {
            if (app == null)
            {
                activeAppId = Int32.MinValue;
                return;
            }

            activeAppId = app.ApplicationID;
            
            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(app, out limits))
            {
                foreach (var limit in limits)
                {
                    var durationTask = workQueue.EnqueueWork(() => trackingService.GetDuration(app, limit.LimitSpan));
                    durationTask.ContinueWith(OnGetAppDuration, limit, CancellationToken.None, 
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
                var notifer = limitNotifiers.Single(l => l.LimitSpan == appLimit.LimitSpan);
                notifer.Limit = appLimit;
                notifer.SetupTimer(new TimeSpan(appLimit.Limit - duration));
            }
        }

        public void Dispose()
        {
            foreach (var notifier in limitNotifiers)
            {
                notifier.Dispose();
            }

            midnightNotifier.Dispose();
        }


        public int InitializationOrder
        {
            get { return 3; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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

        private Boolean isTrackingEnabled;
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
        }

        private void OnLimitReached(object sender, LimitReachedArgs args)
        {
            if (args.Limit != null)
            {
                limitHandler.Handle(args.Limit);
            }
        }


        public void SettingsChanged(Setting settings)
        {
            isTrackingEnabled = settings.TrackingEnabled;
        }


        public void Initialize(Setting settings)
        {
            isTrackingEnabled = settings.TrackingEnabled;

            LoadAppLimits();

            midnightNotifier.MidnightTick += OnMidnightTick;
            appChangedNotifier.AppChanged += OnAppChanged;
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

        private async void OnMidnightTick(object sender, EventArgs e)
        {
            StopNotifiers();
            await CheckLimits();
        }

        private void StopNotifiers()
        {
            foreach (var notifer in limitNotifiers)
            {
                notifer.Stop();
            }
        }


        private async Task CheckLimits()
        {
            foreach (var notifier in limitNotifiers.Where(l => l.Limit != null))
            {
                await LoadAppDurations(notifier.Limit.Application);
            }
        }


        private async void OnAppChanged(object sender, AppChangedArgs e)
        {
            if ((activeAppInfo == e.LogInfo.AppInfo && activeWindowTitle == e.LogInfo.WindowTitle)
                || appLimitsMap.Count == 0 || !isTrackingEnabled)
                return;

            activeAppInfo = e.LogInfo.AppInfo;
            activeWindowTitle = e.LogInfo.WindowTitle;

            StopNotifiers();

            var valueFactory = new Func<Object>(() => trackingService.GetApp(e.LogInfo.AppInfo));
            var app = (Aplication)await workQueue.EnqueueWork(valueFactory);

            if (app != null && app.AppInfo == activeAppInfo)
            {
                await LoadAppDurations(app);
            }
            else
            {
                activeAppId = Int32.MinValue;
            }
        }

        private async Task LoadAppDurations(Aplication app)
        {
            activeAppId = app.ApplicationID;

            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(app, out limits))
            {
                foreach (var limit in limits)
                {
                    var durationTask = workQueue.EnqueueWork(() => trackingService.GetDuration(app, limit.LimitSpan));
                    var duration = (Int64)await workQueue.EnqueueWork(() => trackingService.GetDuration(app, limit.LimitSpan));
                    CheckDuration(limit, duration);
                }
            }
        }

        private void CheckDuration(AppLimit limit, Int64 duration)
        {
            if (duration >= limit.Limit)
            {
                limitHandler.Handle(limit);
            }
            else if (activeAppId != limit.ApplicationID)
            {
                return;
            }
            else
            {
                var notifer = limitNotifiers.Single(l => l.LimitSpan == limit.LimitSpan);
                notifer.Setup(limit, new TimeSpan(limit.Limit - duration));
            }
        }

        public void Dispose()
        {
            foreach (var notifier in limitNotifiers)
            {
                notifier.Dispose();
            }

            midnightNotifier.Dispose();
            appChangedNotifier.Dispose();
        }


        public int InitializationOrder
        {
            get { return 3; }
        }
    }
}

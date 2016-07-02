using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
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
        private readonly IAppDurationCalc appDurationCalc;
        private readonly IMediator mediator;

        private readonly IDictionary<String, IEnumerable<AppLimit>> appLimitsMap
            = new Dictionary<String, IEnumerable<AppLimit>>();

        private readonly ICollection<LimitNotifier> limitNotifiers
            = new List<LimitNotifier>(2);

        private IDisposable subscription;

        private Boolean isTrackingEnabled;
        private String activeAppName;

        [ImportingConstructor]
        public LimitObserver(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IMidnightNotifier midnightNotifier,
                             ILimitHandler limitHandler,
                             IAppDurationCalc appDurationCalc,
                             IMediator mediator,
                             ISyncContext syncContext)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appChangedNotifier = appChangedNotifier;
            this.midnightNotifier = midnightNotifier;
            this.limitHandler = limitHandler;
            this.appDurationCalc = appDurationCalc;
            this.mediator = mediator;

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
            subscription = appChangedNotifier.AppChangedObservable
                .Where(a => appLimitsMap.Count > 0 && isTrackingEnabled)
                .Select(a => a.LogInfo.AppInfo.GetAppName())
                .Do(n =>
                {
                    activeAppName = n;
                    StopNotifiers();
                })
                .Where(n => appLimitsMap.ContainsKey(n))
                .SelectMany(n => Observable.ToObservable(appLimitsMap[n]))
                .SelectMany(l => Observable.FromAsync(async () =>
                {
                    return new
                    {
                        Limit = l,
                        Duration = await appDurationCalc.GetDuration(l.Application.Name, l.LimitSpan)
                    };
                }))
                .Subscribe(r => CheckDuration(r.Limit, r.Duration));

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
                await LoadAppDurations(notifier.Limit.Application.Name);
            }
        }

        private async Task LoadAppDurations(String appName)
        {
            IEnumerable<AppLimit> limits;
            if (appLimitsMap.TryGetValue(appName, out limits))
            {
                foreach (var limit in limits)
                {
                    var duration = await appDurationCalc.GetDuration(appName, limit.LimitSpan);
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
            else if (activeAppName != limit.Application.Name)
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
            subscription.Dispose();
        }


        public int InitializationOrder
        {
            get { return 3; }
        }
    }
}

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Hooks;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class WindowTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IAppChangedNotifier appChangedNotifier;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;

        private IDisposable appChangedSubscription;
        private IDisposable screenshotSubscription;

        private Setting settings;
        private Log activeLog;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appChangedNotifier = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.mediator = mediator;

            appChangedSubscription = appChangedNotifier.AppChangedObservable
                .Where(a => isTrackingEnabled)
                .Select(a => a.LogInfo.AppInfo == AppInfo.Empty
                    || string.IsNullOrEmpty(a.LogInfo.AppInfo.GetAppName()) ? LogInfo.Empty : a.LogInfo)
                .Do(i => FinishActiveLog())
                .Subscribe(CreateActiveLog);

            screenshotSubscription = screenshotTracker.ScreenshotObservable
                .Where(s => s != null && isTrackingEnabled && activeLog != null)
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(s => activeLog.Screenshots.Add(s));
        }

        private void FinishActiveLog()
        {
            if (activeLog != null)
            {
                activeLog.Finish();
                dataService.SaveModifiedEntity(activeLog);
                activeLog = null;
            }
        }

        private void CreateActiveLog(LogInfo logInfo)
        {
            if (logInfo == LogInfo.Empty)
            {
                return;
            }

            bool isNewApp;
            var app = GetApp(logInfo, out isNewApp);
            var window = GetWindow(logInfo, app);

            var log = new Log(window.WindowID, trackingService.UsageID)
            {
                DateCreated = logInfo.Start,
                UtcDateCreated = logInfo.UtcStart,
                DateEnded = logInfo.End,
                UtcDateEnded = logInfo.UtcEnd,
            };

            dataService.SaveNewEntity(log);

            if (isNewApp)
            {
                mediator.NotifyColleagues(MediatorMessages.APPLICATION_ADDED, app);
            }

            activeLog = log;
        }

        private Aplication GetApp(LogInfo logInfo, out bool isNewApp)
        {
            var appName = logInfo.AppInfo.GetAppName();
            var appList = dataService.GetFiltered<Aplication>(a => a.UserID == trackingService.UserID
                                                                        && a.Name == appName);
            var app = appList.FirstOrDefault();
            isNewApp = false;
            if (app == null)
            {
                app = new Aplication(logInfo.AppInfo) { UserID = trackingService.UserID };
                dataService.SaveNewEntity(app);
                isNewApp = true;
            }
            return app;
        }

        private Window GetWindow(LogInfo logInfo, Aplication app)
        {
            var windowsList = dataService.GetFiltered<Window>(w => w.Title == logInfo.WindowTitle
                                                               && w.Application.ApplicationID == app.ApplicationID);
            var window = windowsList.FirstOrDefault();
            if (window == null)
            {
                window = new Window(logInfo.WindowTitle, app.ApplicationID);
                dataService.SaveNewEntity(window);
            }

            return window;
        }

        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
            screenshotTracker.SettingsChanging(settings);
        }

        public void Initialize(Setting settings)
        {
            this.settings = settings;

            screenshotTracker.Initialize(settings);
            
            ConfigureComponents();

            mediator.Register(MediatorMessages.STOP_TRACKING, new Action(StopTracking));
            mediator.Register(MediatorMessages.RESUME_TRACKING, new Action(ResumeTracking));

            if (settings.TrackingEnabled)
            {
                appChangedNotifier.CheckActiveApp();
            }
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled = settings.TrackingEnabled;
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            FinishActiveLog();
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.TrackingEnabled;
            if (isTrackingEnabled)
            {
                appChangedNotifier.CheckActiveApp();
            }
        }

        public void Dispose()
        {
            appChangedNotifier.Dispose();
            screenshotTracker.Dispose();
            StopTracking();
            appChangedSubscription.Dispose();
            screenshotSubscription.Dispose();
        }

        public int InitializationOrder
        {
            get { return 1; }
        }
    }
}

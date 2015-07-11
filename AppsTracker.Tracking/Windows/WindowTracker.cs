#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Tracking.Hooks;
using AppsTracker.Common.Utils;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class WindowTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IAppChangedNotifier appNotifierInstance;
        private readonly ISyncContext syncContext;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;
        private readonly IWorkQueue workQueue;

        private LazyInit<IAppChangedNotifier> appChangedNotifier;

        //private Log activeLog;
        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             ISyncContext syncContext,
                             IMediator mediator,
                             IWorkQueue workQueue)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appNotifierInstance = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.syncContext = syncContext;
            this.mediator = mediator;
            this.workQueue = workQueue;

            activeLogInfo = LogInfo.EmptyLog;
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
            screenshotTracker.ScreenshotTaken += ScreenshotTaken;

            appChangedNotifier = new LazyInit<IAppChangedNotifier>(() => appNotifierInstance,
                                                           a => a.AppChanged += AppChanging,
                                                           a => a.AppChanged -= AppChanging);

            ConfigureComponents();

            mediator.Register(MediatorMessages.STOP_TRACKING, new Action(StopTracking));
            mediator.Register(MediatorMessages.RESUME_TRACKING, new Action(ResumeTracking));
        }

        private void ScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isTrackingEnabled == false || screenshot == null)
                return;

            activeLogInfo.Screenshots.Add(screenshot);
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled =
                appChangedNotifier.Enabled =
                        settings.LoggingEnabled;
        }

        private void AppChanging(object sender, AppChangedArgs e)
        {
            SaveActiveLog(false);
            
            if (!isTrackingEnabled || e.AppInfo == null || 
                (e.AppInfo != null
                && string.IsNullOrEmpty(e.AppInfo.Name)
                && string.IsNullOrEmpty(e.AppInfo.FullName)
                && string.IsNullOrEmpty(e.AppInfo.FileName)))
            {
                return;
            }

            activeLogInfo = new LogInfo(e.AppInfo, e.WindowTitle);
        }

        private void SaveActiveLog(bool shuttingDown)
        {
            var logCopy = activeLogInfo;
            if (logCopy.IsFinished)
                return;

            logCopy.Finish();
            if (shuttingDown)
                trackingService.CreateLogEntry(logCopy);
            else
                workQueue.EnqueueWork(() => trackingService.CreateLogEntry(logCopy));
        }

        private void NewAppAdded(AppInfo appInfo)
        {
            var newApp = trackingService.GetApp(appInfo);
            if (newApp != null)
                mediator.NotifyColleagues(MediatorMessages.APPLICATION_ADDED, newApp);
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            SaveActiveLog(true);
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.LoggingEnabled;
        }

        public void Dispose()
        {
            StopTracking();
            //appNotifierInstance.Dispose();
            screenshotTracker.Dispose();

            appChangedNotifier.Enabled = false;

            workQueue.Dispose();
        }


        public int InitializationOrder
        {
            get { return 1; }
        }

    }
}

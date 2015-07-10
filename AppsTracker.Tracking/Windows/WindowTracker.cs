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

        private LazyInit<IAppChangedNotifier> appChangedNotifier;

        private Log activeLog;
        private Setting settings;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             ISyncContext syncContext,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appNotifierInstance = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.syncContext = syncContext;
            this.mediator = mediator;
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

        private async void ScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isTrackingEnabled == false || screenshot == null || activeLog == null)
                return;

            screenshot.LogID = activeLog.LogID;
            await dataService.SaveNewEntityAsync(screenshot);
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled =
                appChangedNotifier.Enabled =
                        settings.LoggingEnabled;
        }

        private void AppChanging(object sender, AppChangedArgs e)
        {
            if (isTrackingEnabled == false)
                return;

            if (e.AppInfo == null || (e.AppInfo != null
                && string.IsNullOrEmpty(e.AppInfo.Name)
                && string.IsNullOrEmpty(e.AppInfo.FullName)
                && string.IsNullOrEmpty(e.AppInfo.FileName)))
            {
                SaveActiveLog(null);
                return;
            }

            bool newApp = false;
            SaveCreateLog(e.WindowTitle, trackingService.UsageID, trackingService.UserID, e.AppInfo, out newApp);

            if (newApp)
                NewAppAdded(e.AppInfo);
        }

        private void SaveCreateLog(string windowTitle, int usageID, int userID, AppInfo appInfo, out bool newApp)
        {
            var newLog = trackingService.CreateNewLog(windowTitle, usageID, userID, appInfo, out newApp);
            SaveActiveLog(newLog);
        }

        private void SaveActiveLog(Log newLog)
        {
            var tempLog = activeLog;
            activeLog = newLog;

            if (tempLog == null)
                return;

            tempLog.Finish();
            dataService.SaveModifiedEntityAsync(tempLog);
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
            SaveActiveLog(null);
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.LoggingEnabled;
        }

        public void Dispose()
        {
            appNotifierInstance.Dispose();
            screenshotTracker.Dispose();

            appChangedNotifier.Enabled = false;

            StopTracking();
        }


        public int InitializationOrder
        {
            get { return 1; }
        }

        private struct LogInfo
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public DateTime UtcStart { get; set; }
            public DateTime UtcEnd { get; set; }
            public AppInfo AppInfo { get; set; }
            public String WindowTitle { get; set; }
            public Screenshot Screenshot { get; set; }
        }
    }
}

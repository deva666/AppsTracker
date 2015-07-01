#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
using AppsTracker.Hooks;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class WindowTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private string activeWindowTitle;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IWindowChangedNotifier windowNotifierInstance;
        private readonly ISyncContext syncContext;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IWindowHelper windowHelper;
        private readonly IMediator mediator;

        private LazyInit<IWindowChangedNotifier> windowNotifier;
        private LazyInit<System.Threading.Timer> windowCheckTimer;

        private Log currentLog;
        private Setting settings;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IWindowChangedNotifier windowNotifier,
                             IScreenshotTracker screenshotTracker,
                             ISyncContext syncContext,
                             IWindowHelper windowHelper,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.windowNotifierInstance = windowNotifier;
            this.screenshotTracker = screenshotTracker;
            this.syncContext = syncContext;
            this.windowHelper = windowHelper;
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
            screenshotTracker.ScreenshotTaken += OnScreenshotTaken;

            windowNotifier = new LazyInit<IWindowChangedNotifier>(() => windowNotifierInstance,
                                                           w => w.WindowChanged += WindowChanging,
                                                           w => w.WindowChanged -= WindowChanging);

            windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => syncContext.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(1000, 1000)
                                                                        , t => t.Dispose());

            ConfigureComponents();

            mediator.Register(MediatorMessages.STOP_TRACKING, new Action(StopTracking));
            mediator.Register(MediatorMessages.RESUME_TRACKING, new Action(ResumeTracking));
        }

        private async void OnScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isTrackingEnabled == false || screenshot == null || currentLog == null)
                return;

            screenshot.LogID = currentLog.LogID;
            await dataService.SaveNewEntityAsync(screenshot);
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled =
                windowNotifier.Enabled =
                    windowCheckTimer.Enabled =
                        settings.LoggingEnabled;
        }

        private void CheckWindowTitle()
        {
            if (isTrackingEnabled == false)
                return;

            if (activeWindowTitle != windowHelper.GetActiveWindowName())
                OnWindowChange(windowHelper.GetActiveWindowName(), windowHelper.GetActiveWindowAppInfo());
        }

        private void WindowChanging(object sender, WindowChangedArgs e)
        {
            if (isTrackingEnabled == false)
                return;

            OnWindowChange(e.WindowTitle, e.AppInfo);
        }

        private void OnWindowChange(string windowTitle, IAppInfo appInfo)
        {
            if (appInfo == null || (appInfo != null
                && string.IsNullOrEmpty(appInfo.Name)
                && string.IsNullOrEmpty(appInfo.FullName)
                && string.IsNullOrEmpty(appInfo.FileName)))
            {
                ExchangeLogs(null);
                return;
            }

            bool newApp = false;
            SaveCreateLog(windowTitle, trackingService.UsageID, trackingService.UserID, appInfo, out newApp);
            activeWindowTitle = windowTitle;

            if (newApp)
                NewAppAdded(appInfo);
        }


        private void SaveCreateLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            var newLog = trackingService.CreateNewLog(windowTitle, usageID, userID, appInfo, out newApp);
            ExchangeLogs(newLog);
        }

        private void ExchangeLogs(Log newLog)
        {
            Log tempLog;

            activeWindowTitle = string.Empty;

            tempLog = currentLog;
            currentLog = newLog;

            if (tempLog == null)
                return;

            tempLog.Finish();
            dataService.SaveModifiedEntityAsync(tempLog);
        }

        private void NewAppAdded(IAppInfo appInfo)
        {
            var newApp = trackingService.GetApp(appInfo);
            if (newApp != null)
                mediator.NotifyColleagues(MediatorMessages.APPLICATION_ADDED, newApp);
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            ExchangeLogs(null);
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.LoggingEnabled;
        }

        public void Dispose()
        {
            windowNotifierInstance.Dispose();
            screenshotTracker.Dispose();

            windowNotifier.Enabled =
                    windowCheckTimer.Enabled = false;

            StopTracking();
        }


        public int InitializationOrder
        {
            get { return 1; }
        }
    }
}

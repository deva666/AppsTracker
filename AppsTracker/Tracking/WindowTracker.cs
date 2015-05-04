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
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(IModule))]
    internal sealed class WindowTracker : IModule
    {
        private bool isLoggingEnabled;

        private string activeWindowTitle;

        private readonly ILoggingService loggingService;
        private readonly IWindowNotifier windowNotifierInstance;
        private readonly ISyncContext syncContext;
        private readonly IScreenshotTracker screenshotTracker;

        private LazyInit<IWindowNotifier> windowNotifier;

        private Log currentLog;

        private LazyInit<System.Threading.Timer> windowCheckTimer;

        private Setting settings;

        [ImportingConstructor]
        public WindowTracker(IWindowNotifier windowNotifier, ILoggingService loggingService
            , IScreenshotTracker screenshotTracker, ISyncContext syncContext)
        {
            this.loggingService = loggingService;
            this.windowNotifierInstance = windowNotifier;
            this.screenshotTracker = screenshotTracker;
            this.syncContext = syncContext;
        }

        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
            screenshotTracker.SettingsChanging(settings);
        }

        public void InitializeComponent(Setting settings)
        {
            this.settings = settings;

            screenshotTracker.Initialize(settings);
            screenshotTracker.ScreenshotTaken += OnScreenshotTaken;

            windowNotifier = new LazyInit<IWindowNotifier>(() => windowNotifierInstance,
                                                           w => w.WindowChanged += WindowChanging,
                                                           w => w.WindowChanged -= WindowChanging);

            windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => syncContext.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(1000, 1000)
                                                                        , t => t.Dispose());

            ConfigureComponents();

            Mediator.Register(MediatorMessages.STOP_LOGGING, new Action(StopLogging));
            Mediator.Register(MediatorMessages.RESUME_LOGGING, new Action(ResumeLogging));
        }

        private async void OnScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isLoggingEnabled == false || screenshot == null || currentLog == null)
                return;

            screenshot.LogID = currentLog.LogID;
            await loggingService.SaveNewScreenshotAsync(screenshot);
        }

        private void ConfigureComponents()
        {
            isLoggingEnabled =
                windowNotifier.Enabled =
                    windowCheckTimer.Enabled =
                        settings.LoggingEnabled;
        }

        private void CheckWindowTitle()
        {
            if (isLoggingEnabled == false)
                return;

            if (activeWindowTitle != WindowHelper.GetActiveWindowName())
                OnWindowChange(WindowHelper.GetActiveWindowName(), WindowHelper.GetActiveWindowAppInfo());
        }

        private void WindowChanging(object sender, WindowChangedArgs e)
        {
            if (isLoggingEnabled == false)
                return;

            OnWindowChange(e.WindowTitle, e.AppInfo);
        }

        private void OnWindowChange(string windowTitle, IAppInfo appInfo)
        {
            if (appInfo == null || (appInfo != null && string.IsNullOrEmpty(appInfo.Name)))
            {
                ExchangeLogs(null);
                return;
            }

            bool newApp = false;
            SaveCreateLog(windowTitle, loggingService.UsageID, loggingService.UserID, appInfo, out newApp);
            activeWindowTitle = windowTitle;

            if (newApp)
                NewAppAdded(appInfo);
        }


        private void SaveCreateLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            var newLog = loggingService.CreateNewLog(windowTitle, usageID, userID, appInfo, out newApp);
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
            loggingService.SaveModifiedLogAsync(tempLog);
        }

        private void NewAppAdded(IAppInfo appInfo)
        {
            var newApp = loggingService.GetApp(appInfo);
            Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, newApp);
        }

        private void StopLogging()
        {
            isLoggingEnabled = false;
            ExchangeLogs(null);
        }

        private void ResumeLogging()
        {
            isLoggingEnabled = settings.LoggingEnabled;
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        public void Dispose()
        {
            windowNotifierInstance.Dispose();
            screenshotTracker.Dispose();

            windowNotifier.Enabled =
                    windowCheckTimer.Enabled = false;

            StopLogging();
        }
    }
}

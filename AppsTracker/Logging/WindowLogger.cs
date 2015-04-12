#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Hooks;
using AppsTracker.Logging.Helpers;
using AppsTracker.ServiceLocation;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    [Export(typeof(IComponent))]
    internal sealed class WindowLogger : IComponent
    {
        private bool isLoggingEnabled;

        private string activeWindowTitle;

        private readonly ILoggingService loggingService;
        private readonly IWindowNotifier windowNotifierInstance;
        private readonly ISyncContext syncContext;
        private readonly IScreenshotFactory screenshotFactory;

        private LazyInit<IWindowNotifier> windowNotifier;

        private Log currentLog;

        private LazyInit<System.Timers.Timer> screenshotTimer;
        private LazyInit<System.Threading.Timer> windowCheckTimer;

        private Setting settings;

        [ImportingConstructor]
        public WindowLogger(IWindowNotifier windowNotifier,ISyncContext syncContext, 
                            IScreenshotFactory screenshotFactory, ILoggingService loggingService)
        {
            this.loggingService = loggingService;
            this.windowNotifierInstance = windowNotifier;
            this.syncContext = syncContext;
            this.screenshotFactory = screenshotFactory;
        }

        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
        }

        public void InitializeComponent(Setting settings)
        {
            this.settings = settings;

            windowNotifier = new LazyInit<IWindowNotifier>(() => windowNotifierInstance,
                                                           w => w.WindowChanged += WindowChanging,
                                                           w => w.WindowChanged -= WindowChanging);

            screenshotTimer = new LazyInit<System.Timers.Timer>(() => new System.Timers.Timer()
                                                            {
                                                                AutoReset = true,
                                                                Interval = settings.TimerInterval
                                                            },
                                                            OnScreenshotInit,
                                                            OnScreenshotDispose);

            windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => syncContext.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(1000, 1000)
                                                                        , t => t.Dispose());

            ConfigureComponents();

            Mediator.Register(MediatorMessages.STOP_LOGGING, new Action(StopLogging));
            Mediator.Register(MediatorMessages.RESUME_LOGGING, new Action(ResumeLogging));
        }


        private void OnScreenshotInit(System.Timers.Timer timer)
        {
            timer.Enabled = true;
            timer.Elapsed += ScreenshotTick;
        }

        private void OnScreenshotDispose(System.Timers.Timer timer)
        {
            timer.Enabled = false;
            timer.Elapsed -= ScreenshotTick;
        }

        private void ConfigureComponents()
        {
            screenshotTimer.Enabled = (settings.TakeScreenshots && settings.LoggingEnabled);

            if ((settings.TakeScreenshots && settings.LoggingEnabled) && settings.TimerInterval != screenshotTimer.Component.Interval)
                screenshotTimer.Component.Interval = settings.TimerInterval;

            isLoggingEnabled =
                windowNotifier.Enabled =
                    windowCheckTimer.Enabled =
                        settings.LoggingEnabled;
        }

        private void ScreenshotTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            syncContext.Invoke(async () =>
            {
                if (isLoggingEnabled == false)
                    return;

                await AddScreenshot();
            });
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

        private async Task AddScreenshot()
        {
            var dbSizeTask = loggingService.GetDBSizeAsync();
            var screenshot = screenshotFactory.CreateScreenshot();

            if (screenshot == null || currentLog == null)
                return;

            screenshot.LogID = currentLog.LogID;

            await Task.WhenAll(loggingService.SaveNewScreenshotAsync(screenshot), dbSizeTask);
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

            windowNotifier.Enabled =
                screenshotTimer.Enabled =
                    windowCheckTimer.Enabled = false;

            StopLogging();
        }
    }
}

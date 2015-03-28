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
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Hooks;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    [Export(typeof(IComponent))]
    internal sealed class WindowLogger : IComponent, ICommunicator
    {
        private IWindowNotifier windowNotifierInstance;

        private readonly object _lock = new object();

        private bool isLoggingEnabled;

        private string activeWindowTitle;

        private readonly ILoggingService loggingService;

        private LazyInit<IWindowNotifier> windowNotifier;

        private Log currentLog;

        private LazyInit<System.Timers.Timer> screenshotTimer;
        private LazyInit<System.Threading.Timer> windowCheckTimer;

        private Setting settings;

        [ImportingConstructor]
        public WindowLogger(IWindowNotifier windowNotifier)
        {
            loggingService = ServiceFactory.Get<ILoggingService>();
            windowNotifierInstance = windowNotifier;
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

            windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => App.Current.Dispatcher.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(500, 500)
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

            windowNotifier.Enabled =
                isLoggingEnabled =
                    windowCheckTimer.Enabled =
                        settings.LoggingEnabled;
        }

        private async void ScreenshotTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isLoggingEnabled == false)
                return;

            await AddScreenshot();
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
            SaveCurrentLog();

            if (appInfo == null || (appInfo != null && string.IsNullOrEmpty(appInfo.Name)))
                return;

            bool newApp = false;
            CreateNewLog(windowTitle, Globals.UsageID, Globals.UserID, appInfo, out newApp);
            activeWindowTitle = windowTitle;

            if (newApp)
                NewAppAdded(appInfo);
        }

        private void SaveCurrentLog()
        {
            if (currentLog == null)
                return;

            Log tempLog;
            lock (_lock)
            {
                tempLog = currentLog;
                currentLog = null;
            }

            tempLog.Finish();
            loggingService.SaveModifiedLogAsync(tempLog);
        }

        private void CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            lock (_lock)
            {
                currentLog = loggingService.CreateNewLog(windowTitle, usageID, userID, appInfo, out newApp);
            }
        }

        private void NewAppAdded(IAppInfo appInfo)
        {
            var newApp = loggingService.GetApp(appInfo);
            Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, newApp);
        }

        private async Task AddScreenshot()
        {
            var dbSizeAsync = Globals.GetDBSizeAsync();

            Screenshot screenshot = Screenshots.GetScreenshot();
            lock (_lock)
            {
                if (screenshot == null || currentLog == null)
                    return;

                screenshot.LogID = currentLog.LogID;
            }

            await Task.WhenAll(loggingService.SaveNewScreenshotAsync(screenshot), dbSizeAsync);
        }

        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
        }

        private void StopLogging()
        {
            isLoggingEnabled = false;
            SaveCurrentLog();
        }

        private void ResumeLogging()
        {
            isLoggingEnabled = true;
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

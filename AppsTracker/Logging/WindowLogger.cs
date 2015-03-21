#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Timers;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Hooks;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    internal sealed class WindowLogger : IComponent, ICommunicator
    {
        private readonly object @lock = new object();

        private bool isLoggingEnabled;

        private string activeWindowTitle;

        private readonly ILoggingService loggingService;

        private Log currentLog;

        private LazyInit<Timer> screenshotTimer;
        private LazyInit<IHook<WinHookArgs>> winHook;
        private LazyInit<System.Threading.Timer> windowCheckTimer;

        private Setting settings;

        public WindowLogger(Setting settings)
        {
            Ensure.NotNull(settings);
            this.settings = settings;

            loggingService = ServiceFactory.Get<ILoggingService>();

            Mediator.Register(MediatorMessages.STOP_LOGGING, new Action(StopLogging));
            Mediator.Register(MediatorMessages.RESUME_LOGGING, new Action(ResumeLogging));

            InitComponents();
            ConfigureComponents();
        }

        private void InitComponents()
        {
            winHook = new LazyInit<IHook<WinHookArgs>>(() => new WinHook(),
                                                                w => w.HookProc += WindowChanged,
                                                                w => w.HookProc -= WindowChanged);

            screenshotTimer = new LazyInit<Timer>(() => new Timer()
                                                             {
                                                                 AutoReset = true,
                                                                 Interval = settings.TimerInterval
                                                             },
                                                             onInit:
                                                             t =>
                                                             {
                                                                 t.Enabled = true;
                                                                 t.Elapsed += ScreenshotTick;
                                                             },
                                                             onDispose:
                                                             t =>
                                                             {
                                                                 t.Enabled = false;
                                                                 t.Elapsed -= ScreenshotTick;
                                                             });

            windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => App.Current.Dispatcher.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(500, 500)
                                                                        , t => t.Dispose());

        }

        private void ConfigureComponents()
        {
            screenshotTimer.Enabled = (settings.TakeScreenshots && settings.LoggingEnabled);

            if ((settings.TakeScreenshots && settings.LoggingEnabled) && settings.TimerInterval != screenshotTimer.Component.Interval)
                screenshotTimer.Component.Interval = settings.TimerInterval;

            isLoggingEnabled =
                winHook.Enabled =
                    windowCheckTimer.Enabled = settings.LoggingEnabled;
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

        private void WindowChanged(object sender, WinHookArgs e)
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
            lock (@lock)
            {
                tempLog = currentLog;
                currentLog = null;
            }

            tempLog.Finish();
            loggingService.SaveModifiedLogAsync(tempLog);
        }

        private void CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            lock (@lock)
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
            lock (@lock)
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

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_keyboardHook"),
        SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_screenshotTimer"),
        SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_winHook"),
        SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_windowCheckTimer")]
        public void Dispose()
        {
            screenshotTimer.Enabled =
                windowCheckTimer.Enabled =
                    winHook.Enabled = false;

            StopLogging();
        }

        public void SetComponentEnabled(bool enabled)
        {
            isLoggingEnabled = enabled;
        }
    }
}

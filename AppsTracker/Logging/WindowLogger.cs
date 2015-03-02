#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
using AppsTracker.Hooks;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    internal sealed class WindowLogger : IComponent, ICommunicator
    {
        private readonly object _lock = new object();

        private bool _isLoggingEnabled;

        private string _currentWindowTitle;

        private Log _currentLog;

        private LazyInit<Timer> _screenshotTimer;
        private LazyInit<IHook<WinHookArgs>> _winHook;
        private LazyInit<IHook<KeyboardHookArgs>> _keyboardHook;
        private LazyInit<System.Threading.Timer> _windowCheckTimer;

        private Setting _settings;

        public WindowLogger(Setting settings)
        {
            Ensure.NotNull(settings);

            _settings = settings;

            InitComponents();
            ConfigureComponents();
        }

        private void InitComponents()
        {
            Mediator.Register(MediatorMessages.STOP_LOGGING, new Action(StopLogging));
            Mediator.Register(MediatorMessages.RESUME_LOGGING, new Action(ResumeLogging));

            _winHook = new LazyInit<IHook<WinHookArgs>>(() => new WinHook(),
                                                                w => w.HookProc += WindowChanged,
                                                                w => w.HookProc -= WindowChanged);

            _keyboardHook = new LazyInit<IHook<KeyboardHookArgs>>(() => new KeyBoardHook(),
                                                                         k => k.HookProc += KeyPressed,
                                                                         k => k.HookProc -= KeyPressed);

            _screenshotTimer = new LazyInit<Timer>(() => new Timer()
                                                             {
                                                                 AutoReset = true,
                                                                 Interval = _settings.TimerInterval
                                                             },
                                                             t => { t.Enabled = true; t.Elapsed += ScreenshotTick; },
                                                             t => { t.Enabled = false; t.Elapsed -= ScreenshotTick; });

            _windowCheckTimer = new LazyInit<System.Threading.Timer>(() => new System.Threading.Timer(s => App.Current.Dispatcher.Invoke(CheckWindowTitle))
                                                                        , t => t.Change(500, 500)
                                                                        , t => t.Dispose());

        }

        private void ConfigureComponents()
        {
            _keyboardHook.Enabled = (_settings.EnableKeylogger && _settings.LoggingEnabled);
            _screenshotTimer.Enabled = (_settings.TakeScreenshots && _settings.LoggingEnabled);

            if ((_settings.TakeScreenshots && _settings.LoggingEnabled) && _settings.TimerInterval != _screenshotTimer.Component.Interval)
                _screenshotTimer.Component.Interval = _settings.TimerInterval;

            _isLoggingEnabled =
                _winHook.Enabled =
                    _windowCheckTimer.Enabled = _settings.LoggingEnabled;
        }

        private async void ScreenshotTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isLoggingEnabled == false)
                return;

            await AddScreenshot();
        }

        private void CheckWindowTitle()
        {
            if (_isLoggingEnabled == false)
                return;

            if (_currentWindowTitle != WindowHelper.GetActiveWindowName())
                NewLogArrived(WindowHelper.GetActiveWindowName(), WindowHelper.GetActiveWindowAppInfo());
        }

        private void KeyPressed(object sender, KeyboardHookArgs e)
        {
            if (_isLoggingEnabled == false || _currentLog == null)
                return;

            lock (_lock)
            {
                if (e.KeyCode == 8)
                    _currentLog.RemoveLastKeyLogItem();
                else if (e.KeyCode == 0x0D)
                    _currentLog.AppendNewKeyLogLine();
                else
                    _currentLog.AppendKeyLog(e.KeyText);

                _currentLog.AppendKeyLogRaw(e.KeyTextRaw);
            }
        }

        private void WindowChanged(object sender, WinHookArgs e)
        {
            if (_isLoggingEnabled == false)
                return;

            NewLogArrived(e.WindowTitle, e.AppInfo);
        }

        private void NewLogArrived(string windowTitle, IAppInfo appInfo)
        {
            bool newApp = false;

            SaveCurrentLog();

            if (appInfo == null || (appInfo != null && string.IsNullOrEmpty(appInfo.ProcessName)))
                return;

            CreateNewLog(windowTitle, Globals.UsageID, Globals.UserID, appInfo, out newApp);
            _currentWindowTitle = windowTitle;

            if (newApp)
                NewAppAdded(appInfo);
        }

        private void SaveCurrentLog()
        {
            if (_currentLog == null)
                return;

            Log tempLog;
            lock (_lock)
            {
                tempLog = _currentLog;
                _currentLog = null;
            }

            tempLog.Finish();
            using (var context = new AppsEntities())
            {
                context.Entry<Log>(tempLog).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }
        }

        private void CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            using (var context = new AppsEntities())
            {
                newApp = false;
                string appName = (!string.IsNullOrEmpty(appInfo.ProcessName) ? appInfo.ProcessName : !string.IsNullOrEmpty(appInfo.ProcessRealName) ? appInfo.ProcessRealName : appInfo.ProcessFileName);
                Aplication app = context.Applications.FirstOrDefault(a => a.UserID == userID
                                                        && a.Name == appName);

                if (app == null)
                {
                    app = new Aplication(appInfo.ProcessName,
                                         appInfo.ProcessFileName,
                                         appInfo.ProcessVersion,
                                         appInfo.ProcessDescription,
                                         appInfo.ProcessCompany,
                                         appInfo.ProcessRealName) { UserID = userID };
                    context.Applications.Add(app);

                    newApp = true;
                }

                Window window = context.Windows.FirstOrDefault(w => w.Title == windowTitle
                                                     && w.Application.ApplicationID == app.ApplicationID);

                if (window == null)
                {
                    window = new Window(windowTitle) { Application = app };
                    context.Windows.Add(window);
                }

                var log = new Log(window, usageID);
                context.Logs.Add(log);
                context.SaveChanges();

                lock (_lock)
                    _currentLog = log;
            }
        }

        private async Task AddScreenshot()
        {
            var dbSizeAsync = Globals.GetDBSizeAsync(); //check the DB size, this methods fires an event if near the maximum allowed size

            Screenshot screenshot = Screenshots.GetScreenshot();
            lock (_lock)
            {
                if (screenshot == null || _currentLog == null)
                    return;

                screenshot.LogID = _currentLog.LogID;
            }
            using (var context = new AppsEntities())
            {
                context.Screenshots.Add(screenshot);
                context.SaveChanges();
            }

            await dbSizeAsync.ConfigureAwait(true);
        }

        private void NewAppAdded(IAppInfo appInfo)
        {
            using (var context = new AppsEntities())
            {
                var name = !string.IsNullOrEmpty(appInfo.ProcessName) ? appInfo.ProcessName.Truncate(250) : !string.IsNullOrEmpty(appInfo.ProcessRealName) ? appInfo.ProcessRealName.Truncate(250) : appInfo.ProcessFileName.Truncate(250);
                var newApp = context.Applications.First(a => a.Name == name);
                Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, newApp);
            }
        }

        public void SettingsChanged(Setting settings)
        {
            _settings = settings;
            ConfigureComponents();
        }

        private void StopLogging()
        {
            _isLoggingEnabled = false;
            SaveCurrentLog();
        }

        private void ResumeLogging()
        {
            _isLoggingEnabled = true;
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_keyboardHook"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_screenshotTimer"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_winHook"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Setting enabled to false calls Dispose", MessageId = "_windowCheckTimer")]
        public void Dispose()
        {
            _keyboardHook.Enabled =
                _screenshotTimer.Enabled =
                    _windowCheckTimer.Enabled =
                        _winHook.Enabled = false;

            StopLogging();
        }

        public void SetComponentEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
        }

        public void SetKeyboardHookEnabled(bool enabled)
        {
            _keyboardHook.CallOn(k => k.EnableHook(enabled));
        }
    }
}

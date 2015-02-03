#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using AppsTracker.Hooks;
using AppsTracker.Models.Proxy;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;
using AppsTracker.Common.Utils;
using AppsTracker.DAL;

namespace AppsTracker.Logging
{
    internal sealed class WindowLogger : IComponent, ICommunicator
    {
        private bool _isLoggingEnabled;

        private string _currentWindowTitle;

        private Log _currentLog;

        private LazyInit<Timer> _screenshotTimer;
        private LazyInit<IHook<WinHookArgs>> _winHook;
        private LazyInit<IHook<KeyboardHookArgs>> _keyboardHook;
        private LazyInit<System.Threading.Timer> _windowCheckTimer;

        private ISettings _settings;

        public WindowLogger(ISettings settings)
        {
            Ensure.NotNull(settings);

            _settings = settings;

            Init();
            Configure();
        }

        private void Init()
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

            if (e.KeyCode == 8)
                _currentLog.RemoveLastKeyLogItem();
            else if (e.KeyCode == 0x0D)
                _currentLog.AppendNewKeyLogLine();
            else
                _currentLog.AppendKeyLog(e.String);

            if (e.KeyCode == 8) _currentLog.AppendKeyLogRaw("Backspace"); //  backspace
            else if (e.KeyCode == 9) _currentLog.AppendKeyLogRaw("Tab"); //  tab
            else if (e.KeyCode == 13) _currentLog.AppendKeyLogRaw("Enter"); //  enter
            else if (e.KeyCode == 16) _currentLog.AppendKeyLogRaw("Shift"); //  shift
            else if (e.KeyCode == 17) _currentLog.AppendKeyLogRaw("Ctrl"); //  ctrl
            else if (e.KeyCode == 18) _currentLog.AppendKeyLogRaw("Alt"); //  alt
            else if (e.KeyCode == 19) _currentLog.AppendKeyLogRaw("Pause"); //  pause/break
            else if (e.KeyCode == 20) _currentLog.AppendKeyLogRaw("Caps lock"); //  caps lock
            else if (e.KeyCode == 27) _currentLog.AppendKeyLogRaw("Escape"); //  escape
            else if (e.KeyCode == 33) _currentLog.AppendKeyLogRaw("Page up"); // page up, to avoid displaying alternate character and confusing people
            else if (e.KeyCode == 34) _currentLog.AppendKeyLogRaw("Page down"); // page down
            else if (e.KeyCode == 35) _currentLog.AppendKeyLogRaw("End"); // end
            else if (e.KeyCode == 36) _currentLog.AppendKeyLogRaw("Home"); // home
            else if (e.KeyCode == 37) _currentLog.AppendKeyLogRaw("Left arrow"); // left arrow
            else if (e.KeyCode == 38) _currentLog.AppendKeyLogRaw("Up arrow"); // up arrow
            else if (e.KeyCode == 39) _currentLog.AppendKeyLogRaw("Right arrow"); // right arrow
            else if (e.KeyCode == 40) _currentLog.AppendKeyLogRaw("Down arrow"); // down arrow
            else if (e.KeyCode == 45) _currentLog.AppendKeyLogRaw("Insert"); // insert
            else if (e.KeyCode == 46) _currentLog.AppendKeyLogRaw("Delete"); // delete
            else if (e.KeyCode == 91) _currentLog.AppendKeyLogRaw("Left window"); // left window
            else if (e.KeyCode == 92) _currentLog.AppendKeyLogRaw("Right window"); // right window
            else if (e.KeyCode == 93) _currentLog.AppendKeyLogRaw("Select key"); // select key
            else if (e.KeyCode == 96) _currentLog.AppendKeyLogRaw("Numpad 0"); // numpad 0
            else if (e.KeyCode == 97) _currentLog.AppendKeyLogRaw("Numpad 1"); // numpad 1
            else if (e.KeyCode == 98) _currentLog.AppendKeyLogRaw("Numpad 2"); // numpad 2
            else if (e.KeyCode == 99) _currentLog.AppendKeyLogRaw("Numpad 3"); // numpad 3
            else if (e.KeyCode == 100) _currentLog.AppendKeyLogRaw("Numpad 4"); // numpad 4
            else if (e.KeyCode == 101) _currentLog.AppendKeyLogRaw("Numpad 5"); // numpad 5
            else if (e.KeyCode == 102) _currentLog.AppendKeyLogRaw("Numpad 6"); // numpad 6
            else if (e.KeyCode == 103) _currentLog.AppendKeyLogRaw("Numpad 7"); // numpad 7
            else if (e.KeyCode == 104) _currentLog.AppendKeyLogRaw("Numpad 8"); // numpad 8
            else if (e.KeyCode == 105) _currentLog.AppendKeyLogRaw("Numpad 9"); // numpad 9
            else if (e.KeyCode == 106) _currentLog.AppendKeyLogRaw("Multiply"); // multiply
            else if (e.KeyCode == 107) _currentLog.AppendKeyLogRaw("Add"); // add
            else if (e.KeyCode == 109) _currentLog.AppendKeyLogRaw("Subtract"); // subtract
            else if (e.KeyCode == 110) _currentLog.AppendKeyLogRaw("Decimal point"); // decimal point
            else if (e.KeyCode == 111) _currentLog.AppendKeyLogRaw("Divide"); // divide
            else if (e.KeyCode == 112) _currentLog.AppendKeyLogRaw("F1"); // F1
            else if (e.KeyCode == 113) _currentLog.AppendKeyLogRaw("F2"); // F2
            else if (e.KeyCode == 114) _currentLog.AppendKeyLogRaw("F3"); // F3
            else if (e.KeyCode == 115) _currentLog.AppendKeyLogRaw("F4"); // F4
            else if (e.KeyCode == 116) _currentLog.AppendKeyLogRaw("F5"); // F5
            else if (e.KeyCode == 117) _currentLog.AppendKeyLogRaw("F6"); // F6
            else if (e.KeyCode == 118) _currentLog.AppendKeyLogRaw("F7"); // F7
            else if (e.KeyCode == 119) _currentLog.AppendKeyLogRaw("F8"); // F8
            else if (e.KeyCode == 120) _currentLog.AppendKeyLogRaw("F9"); // F9
            else if (e.KeyCode == 121) _currentLog.AppendKeyLogRaw("F10"); // F10
            else if (e.KeyCode == 122) _currentLog.AppendKeyLogRaw("F11"); // F11
            else if (e.KeyCode == 123) _currentLog.AppendKeyLogRaw("F12"); // F12
            else if (e.KeyCode == 144) _currentLog.AppendKeyLogRaw("Num lock"); // num lock
            else if (e.KeyCode == 145) _currentLog.AppendKeyLogRaw("Scroll lock"); // scroll lock
            else _currentLog.AppendKeyLogRaw(e.KeyName);
        }

        private void WindowChanged(object sender, WinHookArgs e)
        {
            if (e.AppInfo == null || string.IsNullOrEmpty(e.AppInfo.ProcessName) || _isLoggingEnabled == false)
                return;

            NewLogArrived(e.WindowTitle, e.AppInfo);
        }

        private void NewLogArrived(string windowTitle, IAppInfo appInfo)
        {
            if (appInfo == null)
            {
                SaveOldLog();
                return;
            }

            bool newApp;
            SaveOldLog();
            _currentLog = CreateNewLog(windowTitle, Globals.UsageID, Globals.UserID, appInfo, out newApp);
            _currentWindowTitle = windowTitle;
            if (newApp)
                NewAppAdded(appInfo);
        }
        private void SaveOldLog()
        {
            if (_currentLog != null)
            {
                _currentLog.Finish();
                using (var context = new AppsEntities())
                {
                    context.Entry<Log>(_currentLog).State = System.Data.Entity.EntityState.Added;
                    context.SaveChanges();
                }
                _currentLog = null;
            }
        }

        private Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
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

                context.SaveChanges();

                return new Log(window.WindowID, usageID);
            }
        }

        private async Task AddScreenshot()
        {
            var dbSizeAsync = Globals.GetDBSizeAsync(); //check the DB size, this methods fires an event if near the maximum allowed size

            Log log = _currentLog;
            Screenshot screenshot = Screenshots.GetScreenshot();

            if (screenshot == null || log == null)
                return;
            log.Screenshots.Add(screenshot);

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

        public void SettingsChanged(ISettings settings)
        {
            _settings = settings;
            Configure();
        }

        private void Configure()
        {
            _keyboardHook.Enabled = (_settings.EnableKeylogger && _settings.LoggingEnabled);
            _screenshotTimer.Enabled = (_settings.TakeScreenshots && _settings.LoggingEnabled);

            if ((_settings.TakeScreenshots && _settings.LoggingEnabled) && _settings.TimerInterval != _screenshotTimer.Component.Interval)
                _screenshotTimer.Component.Interval = _settings.TimerInterval;

            _isLoggingEnabled =
                _winHook.Enabled =
                    _windowCheckTimer.Enabled = _settings.LoggingEnabled;
        }

        private void StopLogging()
        {
            _isLoggingEnabled = false;
            SaveOldLog();
            _currentLog = null;
        }

        private void ResumeLogging()
        {
            _isLoggingEnabled = true;
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }

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

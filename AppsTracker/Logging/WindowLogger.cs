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

namespace AppsTracker.Logging
{
    internal sealed class WindowLogger : IComponent, ICommunicator
    {
        bool _isLoggingEnabled;

        Log _currentLog;

        ServiceWrap<Timer> _screenshotTimer;
        ServiceWrap<IHook<WinHookArgs>> _winHook;
        ServiceWrap<IHook<KeyboardHookArgs>> _keyboardHook;

        IAppsService _service;
        ISettings _settings;

        public WindowLogger(ISettings settings)
        {
            Ensure.NotNull(settings);

            _service = ServiceFactory.Get<IAppsService>();
            _settings = settings;
            Init();
            Configure();
        }

        private void Init()
        {
            Mediator.Register(MediatorMessages.IdleEntered, new Action(StopLogging));
            Mediator.Register(MediatorMessages.IdleStopped, new Action(ResumeLogging));

            _winHook = new ServiceWrap<IHook<WinHookArgs>>(() => new WinHook(),
                                                                w => w.HookProc += WindowChanged,
                                                                w => w.HookProc -= WindowChanged) { Enabled = _settings.LoggingEnabled };
            _keyboardHook = new ServiceWrap<IHook<KeyboardHookArgs>>(() => new KeyBoardHook(),
                                                                         k => k.HookProc += KeyPressed,
                                                                         k => k.HookProc -= KeyPressed);
            _screenshotTimer = new ServiceWrap<Timer>(() => new Timer()
                                                             {
                                                                 AutoReset = true,
                                                                 Interval = _settings.TimerInterval
                                                             },
                                                             t => t.Elapsed += ScreenshotTick,
                                                             t => t.Elapsed -= ScreenshotTick);

        }

        private void ScreenshotTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isLoggingEnabled == false)
                return;

            AddScreenshot();
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

            bool newApp;
            AddLog();
            _currentLog = _service.CreateNewLog(e.WindowTitle, Globals.UsageID, Globals.UserID, e.AppInfo, out newApp);
            if (newApp)
                NewAppAdded(e.AppInfo);
        }

        private async void AddScreenshot()
        {
            var dbSizeAsync = Globals.GetDBSizeAsync();

            Log log = _currentLog;
            Screenshot screenshot = Screenshots.GetScreenshot();

            if (screenshot == null || log == null)
                return;
            log.Screenshots.Add(screenshot);

            await dbSizeAsync.ConfigureAwait(true); //check the DB size, this methods fires an event if near the maximum allowed size
        }

        private void AddLog()
        {
            if (_currentLog != null)
            {
                _currentLog.Finish();
                _service.Add<Log>(_currentLog);
            }
        }

        private void NewAppAdded(IAppInfo appInfo)
        {
            var newApp = _service.GetSingle<Aplication>(a => a.Name == appInfo.ProcessName, a => a.Windows);
            Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, newApp);
        }

        public void SettingsChanged(ISettings settings)
        {
            _settings = settings;
            Configure();
        }

        private void Configure()
        {
            if (_keyboardHook.Enabled != (_settings.EnableKeylogger && _settings.LoggingEnabled))
                _keyboardHook.Enabled = (_settings.EnableKeylogger && _settings.LoggingEnabled);
            if (_screenshotTimer.Enabled != (_settings.TakeScreenshots && _settings.LoggingEnabled))
                _screenshotTimer.Enabled = (_settings.TakeScreenshots && _settings.LoggingEnabled);
            if ((_settings.TakeScreenshots && _settings.LoggingEnabled) && _settings.TimerInterval != _screenshotTimer.Component.Interval)
                _screenshotTimer.Component.Interval = _settings.TimerInterval;
            if (_settings.LoggingEnabled != _isLoggingEnabled)
                _isLoggingEnabled = _winHook.Enabled = _settings.LoggingEnabled;
        }

        private void StopLogging()
        {
            _isLoggingEnabled = false;
            AddLog();
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
                    _winHook.Enabled = false;
        }

        public void SetLoggingEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
        }

        public void SetKeyboardHookEnabled(bool enabled)
        {
            _keyboardHook.CallOnService(k => k.EnableHook(enabled));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task_Logger_Pro.Hooks;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.Logging
{
    public sealed class DataLogger : IDisposable, ICommunicator
    {
        #region Fields

        bool _disposed = false;
        bool _loggingStoped = false;
        bool _enableIdle;
        bool _enableKeyboardHook;
        bool _takeScreenShots;
        bool _enableFileWatcher;

        double _screenshotInterval = 2 * 60 * 1000;

        string _currentWindowTitle;

        LoggingStatus _currentLoggingStatus;

        System.Timers.Timer _screenshotTimer;

        System.Threading.Timer _windowCheckTimer;

        IdleMonitor _idleMonitor;

        FileWatcher _fileSystemWatcher;

        KeyBoardHook _keyBoardHook;

        WinEvent _winEvent;

        ProcessKiller _processKiller;

        Log _currentLog;

        Usage _currentUsageLocked;
        Usage _currentUsageIdle;
        Usage _currentUsageLogin;
        Usage _currentUsageStopped;

        #endregion

        #region Properties

        public bool IsLogggingStopped
        {
            get
            {
                return _loggingStoped;
            }
            set
            {
                Console.WriteLine("IS LOGGING STOPPED = " + value);
                _loggingStoped = value;
                if (_idleMonitor != null)
                    _idleMonitor.Enabled = !value;
            }
        }

        public double ScreenShotInterval
        {
            get
            {
                return _screenshotInterval;
            }
            set
            {
                _screenshotInterval = value;
                if (_screenshotTimer != null)
                    _screenshotTimer.Interval = value;
            }

        }
        public bool TakeScreenShots
        {
            get
            {
                return _takeScreenShots;
            }
            set
            {
                if (value != _takeScreenShots)
                {
                    _takeScreenShots = value;
                    if (_takeScreenShots)
                        InitScreenshotTimer();
                    else
                        DisposeScreenshotTimer();
                }
            }
        }
        public bool EnableIdle
        {
            get
            {
                return _enableIdle;
            }
            set
            {
                _enableIdle = value;
                if (_enableIdle)
                    InitIdleMonitor();
                else
                    DisposeIdleMonitor();
            }
        }

        public bool EnableKeyboardHook
        {
            get
            {
                return _enableKeyboardHook;
            }
            set
            {
                if (value != _enableKeyboardHook)
                {
                    _enableKeyboardHook = value;
                    if (_enableKeyboardHook)
                        InitKeyboardHook();
                    else
                        DisposeKeyboardHook();
                }
            }
        }

        public FileSystemWatcher FileSystemWatcher
        {
            get
            {
                return _fileSystemWatcher;
            }
        }

        public bool EnableFileWatcher
        {
            get
            {
                return _enableFileWatcher;
            }
            set
            {
                _enableFileWatcher = value;
                if (_enableFileWatcher)
                    InitFileWatcher();
                else
                    DisposeFileWatcher();
            }
        }

        public KeyBoardHook KeyBoardHook
        {
            get
            {
                return _keyBoardHook;
            }
        }
        private Log CurrentLog
        {
            get
            {
                return _currentLog;
            }
            set
            {
                if (_currentLog != null && !_currentLog.Finished)
                    EndSaveLog(_currentLog).Wait();
                if (_currentLog != value)
                    _currentLog = value;
            }

        }
        public LoggingStatus LoggingStatus
        {
            get
            {
                return _currentLoggingStatus;
            }
            set
            {
                if (_currentLoggingStatus != value)
                {
                    _currentLoggingStatus = value;

                    if (value == Task_Logger_Pro.LoggingStatus.Running)
                    {
                        if (IsLogggingStopped)
                            ResumeLogging();
                        LoggingResumed();
                    }
                    else
                    {
                        if (!IsLogggingStopped)
                            StopLogging(_currentLog).Wait();
                        LoggingStopped();

                    }

                    (App.Current as App).ChangeTheme();
                }
            }
        }

        #endregion

        #region Constructor

        public DataLogger(UzerSetting settings)
        {
            CreateLoadUser();

            _currentLoggingStatus = settings.LoggingEnabled.ConvertToLoggingStatus();
            IsLogggingStopped = !settings.LoggingEnabled;

            this.EnableIdle = settings.EnableIdle;
            this.EnableKeyboardHook = settings.EnableKeylogger;
            this.TakeScreenShots = settings.TakeScreenshots;
            this.ScreenShotInterval = settings.TimerInterval;
            this.EnableFileWatcher = settings.EnableFileWatcher;

            _winEvent = new WinEvent();
            _winEvent.ActiveWindowChanged += ActiveWindowChangedEventHandler;
            _windowCheckTimer = new System.Threading.Timer((o) => App.Current.Dispatcher.Invoke(CheckWindowTitle), null, 500, 500);

            CheckBlockedApps();

            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<List<AppsToBlock>>(AppsToBlockChanging));

            Globals.GetDBSize();
        }

        #endregion

        #region Init & Dispose Methods

        private void InitKeyboardHook()
        {
            if (_keyBoardHook != null)
                return;
            _keyBoardHook = new KeyBoardHook();
            _keyBoardHook.KeyDown += KeyDownEventHandler;
            _keyBoardHook.KeyPress += KeyPressEventHandler;
        }

        private void DisposeKeyboardHook()
        {
            if (_keyBoardHook == null)
                return;
            _keyBoardHook.KeyDown -= KeyDownEventHandler;
            _keyBoardHook.KeyPress -= KeyPressEventHandler;
            _keyBoardHook.Dispose();
            _keyBoardHook = null;
        }

        private void InitScreenshotTimer()
        {
            if (_screenshotTimer != null)
                return;
            _screenshotTimer = new System.Timers.Timer();
            _screenshotTimer.AutoReset = true;
            _screenshotTimer.Enabled = true;
            _screenshotTimer.Elapsed += TimerElapsedEventHandler;
            _screenshotTimer.Interval = _screenshotInterval;
        }

        private void DisposeScreenshotTimer()
        {
            if (_screenshotTimer == null)
                return;
            _screenshotTimer.Enabled = false;
            _screenshotTimer.Elapsed -= TimerElapsedEventHandler;
            _screenshotTimer.Dispose();
            _screenshotTimer = null;
        }

        private void InitFileWatcher()
        {
            if (_fileSystemWatcher != null)
                return;
            _fileSystemWatcher = new FileWatcher();
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Created += watcher_Changed;
            _fileSystemWatcher.Deleted += watcher_Changed;
            _fileSystemWatcher.Renamed += watcher_Renamed;
            _fileSystemWatcher.Path = App.UzerSetting.FileWatcherPath;
            _fileSystemWatcher.EnableRaisingEvents = true;
            Console.WriteLine("INITIALIZED");
        }

        private void DisposeFileWatcher()
        {
            if (_fileSystemWatcher == null)
                return;
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Created -= watcher_Changed;
            _fileSystemWatcher.Deleted -= watcher_Changed;
            _fileSystemWatcher.Renamed -= watcher_Renamed;
            _fileSystemWatcher.Dispose();
            _fileSystemWatcher = null;
            Console.WriteLine("DISPOSED");
        }

        private void InitIdleMonitor()
        {
            if (_idleMonitor != null)
                return;
            _idleMonitor = new IdleMonitor();
            _idleMonitor.IdleEntered += IdleEntered;
            _idleMonitor.IdleStoped += IdleStoped;
            _idleMonitor.Enabled = !_loggingStoped;
        }

        private void DisposeIdleMonitor()
        {
            if (_idleMonitor == null)
                return;
            _idleMonitor.IdleEntered -= IdleEntered;
            _idleMonitor.IdleStoped -= IdleStoped;
            _idleMonitor.Dispose();
            _idleMonitor = null;
        }

        #endregion

        #region Event Handlers

        private void KeyDownEventHandler(object sender, KeyboardHookEventArgs e)
        {
            if (IsLogggingStopped)
                return;

            CheckWindowTitle();

            if (_currentLog != null)
            {
                if (e.KeyCode == 8)
                    _currentLog.RemoveLastKeyLogItem();
                else if (e.KeyCode == 0x0D)
                    _currentLog.AppendNewKeyLogLine();
                else
                    _currentLog.AppendKeyLog(e.String);
            }
        }

        private void KeyPressEventHandler(object sender, KeyboardHookEventArgs e)
        {
            if (IsLogggingStopped)
                return;

            CheckWindowTitle();

            if (_currentLog != null)
            {
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
        }

        private void TimerElapsedEventHandler(object sender, EventArgs e)
        {
            if (IsLogggingStopped)
                return;

            AddScreenShot();
        }

        private void ActiveWindowChangedEventHandler(object sender, WinEventArgs e)
        {
            if (IsLogggingStopped)
                return;

            CurrentLog = NewWindowEvent(e);
        }

        private void CheckWindowTitle()
        {
            if (IsLogggingStopped)
                return;
            if (_currentWindowTitle != _winEvent.GetActiveWindowName())
                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (IsLogggingStopped)
                return;

            Models.FileLog fileLog = new Models.FileLog(e.OldFullPath, e.ChangeType.ToString(), e.FullPath, Globals.UserID);
            using (var context = new AppsEntities1())
            {
                context.FileLogs.Add(fileLog);
                context.SaveChanges();
            }

        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("FILE CHANGED");
            if (IsLogggingStopped)
                return;

            Models.FileLog fileLog = new Models.FileLog(e.FullPath, e.ChangeType.ToString(), Globals.UserID);
            using (var context = new AppsEntities1())
            {
                context.FileLogs.Add(fileLog);
                context.SaveChanges();
            }

        }

        private void _processKiller_ProcessKilledEvent(object sender, ProcessKilledEventArgs e)
        {
            using (var context = new AppsEntities1())
            {
                BlockedApp blockedApp = new BlockedApp() { Date = DateTime.Now, ApplicationID = e.Aplication.ApplicationID, UserID = Globals.UserID };
                context.BlockedApps.Add(blockedApp);
                context.SaveChanges();
            }
        }

        private async void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                var taskStopLogging = StopLogging(CurrentLog);
                Debug.Assert(_currentUsageLocked == null, "CURRENT USAGE LOCKED NOT NULL (FAILED TO CATCH UNLOCK EVENT)");
                _currentUsageLocked = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
                if (_currentUsageIdle != null)
                {
                    string usageType = UsageTypes.Idle.ToString();
                    _currentUsageIdle.UsageEnd = DateTime.Now;
                    await SaveUsage(usageType, _currentUsageIdle);
                    _currentUsageIdle = null;
                }
                StartStopIdleMonitor(false);
                taskStopLogging.Wait();
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                ResumeLogging();
                StartStopIdleMonitor(true);
                if (_currentUsageLocked != null)
                {
                    string usageType = UsageTypes.Locked.ToString();
                    _currentUsageLocked.UsageEnd = DateTime.Now;
                    await SaveUsage(usageType, _currentUsageLocked);
                    _currentUsageLocked = null;
                }
            }
        }

        private void StartStopIdleMonitor(bool enabled)
        {
            if (_idleMonitor == null)
                return;
            _idleMonitor.Enabled = enabled; 
        }

        private async Task SaveUsage(string usageType, Usage usage)
        {
            using (var context = new AppsEntities1())
            {
                var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
                usage.UsageTypeID = usageLockedID;
                context.Usages.Add(usage);
                await context.SaveChangesAsync();
            }
        }
        private async void IdleEntered(object sender, EventArgs e)
        {
            if (IsLogggingStopped)
                return;
            Debug.Assert(_currentUsageIdle == null, "CURRENT USAGE IDLE NOT NULL");
            _currentUsageIdle = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
            await StopLogging(CurrentLog);
        }

        private void IdleStoped(object sender, EventArgs e)
        {
            Console.WriteLine("IDLE STOPPED");
            ResumeLogging();
            if (_currentUsageIdle != null)
            {
                _currentUsageIdle.UsageEnd = DateTime.Now;
                using (var context = new AppsEntities1())
                {
                    string usageType = UsageTypes.Idle.ToString();
                    var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
                    _currentUsageIdle.UsageTypeID = usageLockedID;
                    context.Usages.Add(_currentUsageIdle);
                    context.SaveChanges();
                }
                _currentUsageIdle = null;
            }
        }

        #endregion

        #region Class Methods

        //private void AddIdleEventHandlers()
        //{
        //    if (_idleMonitor == null)
        //        return;
        //    _idleMonitor.IdleEntered += IdleEntered;
        //    _idleMonitor.IdleStoped += IdleStoped;
        //}

        //private void RemoveIdleEventHandlers()
        //{
        //    if (_idleMonitor == null)
        //        return;
        //    _idleMonitor.IdleEntered -= IdleEntered;
        //    _idleMonitor.IdleStoped -= IdleStoped;
        //    _idleMonitor.IdleEntered -= IdleEntered;
        //    _idleMonitor.IdleStoped -= IdleStoped;
        //    _idleMonitor.IdleEntered -= IdleEntered;
        //    _idleMonitor.IdleStoped -= IdleStoped;
        //}

        private void LoggingStopped()
        {
            if (_currentUsageStopped == null)
                _currentUsageStopped = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };

            StartStopIdleMonitor(false);

        }

        private void LoggingResumed()
        {
            if (_currentUsageStopped != null)
            {
                _currentUsageStopped.UsageEnd = DateTime.Now;

                using (var context = new AppsEntities1())
                {
                    string usageType = UsageTypes.Stopped.ToString();
                    var usageStoppedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
                    _currentUsageStopped.UsageTypeID = usageStoppedID;
                    context.Usages.Add(_currentUsageStopped);
                    context.SaveChanges();
                }
                _currentUsageStopped = null;
            }

            StartStopIdleMonitor(true);
        }

        private void AppsToBlockChanging(List<AppsToBlock> appsToBlockList)
        {
            if (appsToBlockList.Count == 0 && _processKiller != null)
            {
                _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;
                _processKiller.Dispose();
                _processKiller = null;
                return;
            }
            else if (_processKiller == null)
            {
                _processKiller = new ProcessKiller();
                _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
            }
            _processKiller.AppsToBlockList = appsToBlockList;
        }

        private void CreateLoadUser()
        {
            Uzer user;
            using (var context = new AppsEntities1())
            {
                if (!context.Users.CheckIfUserExists(Environment.UserName))
                {
                    user = new Uzer() { Name = Environment.UserName };
                    context.Users.Add(user);
                }
                else
                    user = context.Users.FirstOrDefault(u => u.Name == Environment.UserName);

                string filterUsage = UsageTypes.Login.ToString();

                if (context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == filterUsage).Count() > 0)
                {
                    var failedSaveUsage = context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == filterUsage).ToList();
                    foreach (var usage in failedSaveUsage)
                    {
                        var lastLog = context.Logs.Where(l => l.UsageID == usage.UsageID).OrderByDescending(l => l.DateCreated).FirstOrDefault();
                        var lastUsage = context.Usages.Where(u => u.SelfUsageID == usage.UsageID).OrderByDescending(u => u.UsageEnd).FirstOrDefault();

                        DateTime lastLogDate = DateTime.MinValue;
                        DateTime lastUsageDate = DateTime.MinValue;

                        if (lastLog != null)
                            lastLogDate = lastLog.DateEnded;

                        if (lastUsage != null)
                            lastUsageDate = lastUsage.UsageEnd;


                        usage.UsageEnd = lastLogDate == lastUsageDate ? usage.UsageEnd : lastUsageDate > lastLogDate ? lastUsageDate : lastLogDate;
                        usage.IsCurrent = false;
                        context.Entry(usage).State = EntityState.Modified;
                    }
                }

                _currentUsageLogin = new Usage(user.UserID) { UsageEnd = DateTime.Now, UsageTypeID = context.UsageTypes.First(u => u.UType == filterUsage).UsageTypeID, IsCurrent = true };

                context.Usages.Add(_currentUsageLogin);
                context.SaveChanges();
                Globals.Initialize(user, _currentUsageLogin.UsageID, context);
            }
        }

        internal void DetachEventHandlers()
        {
            if (_keyBoardHook != null)
            {
                _keyBoardHook.KeyPress -= KeyPressEventHandler;
                _keyBoardHook.KeyDown -= KeyDownEventHandler;
            }

            if (_winEvent != null)
                _winEvent.ActiveWindowChanged -= ActiveWindowChangedEventHandler;

            if (_screenshotTimer != null)
                _screenshotTimer.Elapsed -= TimerElapsedEventHandler;
            DetachFileWatcherEventHandlers();

            if (_processKiller != null)
                _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;

            if (_idleMonitor != null)
            {
                _idleMonitor.IdleEntered -= IdleEntered;
                _idleMonitor.IdleStoped -= IdleStoped;
            }
            Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        }

        private void DetachFileWatcherEventHandlers()
        {
            if (_fileSystemWatcher != null)
            {
                _fileSystemWatcher.Created -= watcher_Changed;
                _fileSystemWatcher.Deleted -= watcher_Changed;
                _fileSystemWatcher.Renamed -= watcher_Renamed;
            }
        }

        private void CheckBlockedApps()
        {
            using (var context = new AppsEntities1())
            {
                var appsToBlock = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Count();

                if (appsToBlock > 0)
                {
                    if (_processKiller == null)
                        _processKiller = new ProcessKiller();
                    _processKiller.AppsToBlockList = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Include(a => a.Application).ToList();
                    _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
                }
            }
        }

        private void AddScreenShot()
        {
            CheckWindowTitle();

            Log log = CurrentLog;
            Screenshot screenshot;

            screenshot = Screenshots.GetScreenshot();
            if (screenshot == null || log == null)
                return;
            log.Screenshots.Add(screenshot);
        }

        #endregion

        #region Logging Methods

        private Log NewWindowEvent(WinEventArgs e)
        {
            if (e.ProcessInfo == null || string.IsNullOrEmpty(e.ProcessInfo.ProcessName))
                return null;

            using (var context = new AppsEntities1())
            {
                bool newApp = false;

                Aplication app = (from a in context.Applications
                                  where a.UserID == Globals.UserID
                                  && a.Name == e.ProcessInfo.ProcessName
                                  select a).FirstOrDefault();

                if (app == null)
                {
                    app = new Aplication(e.ProcessInfo.ProcessName,
                                                    e.ProcessInfo.ProcessFileName,
                                                    e.ProcessInfo.ProcessVersion,
                                                    e.ProcessInfo.ProcessDescription,
                                                    e.ProcessInfo.ProcessCompany,
                                                    e.ProcessInfo.ProcessRealName) { UserID = Globals.UserID };
                    newApp = true;
                    context.Applications.Add(app);
                    context.SaveChanges();
                }
                if (app.ApplicationID == 0)
                    throw new InvalidOperationException("Application ID is 0, " + app.Name);

                Window window = (from w in context.Windows
                                 where w.Title == e.WindowTitle
                                 && w.ApplicationID == app.ApplicationID
                                 select w).FirstOrDefault();

                if (window == null)
                {
                    window = new Window() { Title = e.WindowTitle, ApplicationID = app.ApplicationID };
                    context.Windows.Add(window);
                    context.SaveChanges();
                }

                if (window.WindowID == 0)
                    throw new InvalidOperationException("Window ID is 0, " + window.Title);

                _currentWindowTitle = e.WindowTitle;

                if (newApp)
                {
                    context.Entry(app).Collection(a => a.Windows).Load();
                    Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, app);
                }

                return new Log(window.WindowID, Globals.UsageID);
            }
        }

        private async Task EndSaveLog(Log log)
        {
            if (!log.Finished)
                log.Finish();

            using (var context = new AppsEntities1())
            {
                context.Logs.Add(log);
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (System.Data.Entity.Core.OptimisticConcurrencyException)
                {
                    context.Entry<Log>(log).Reload();
                    context.SaveChanges();
                }
            }
        }

        private async Task StopLogging(Log log)
        {
            IsLogggingStopped = true;
            if (log != null)
                await EndSaveLog(log);

            _currentWindowTitle = string.Empty;
            _windowCheckTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            _currentLog = null;
        }

        private void ResumeLogging()
        {
            IsLogggingStopped = false;
            _windowCheckTimer.Change(500, 500);
            CheckWindowTitle();
        }

        internal void FinishLogging()
        {
            Task stopTask = StopLogging(CurrentLog);

            using (var context = new AppsEntities1())
            {
                _currentUsageLogin.UsageEnd = DateTime.Now;
                _currentUsageLogin.IsCurrent = false;
                if (_currentUsageStopped != null)
                {
                    _currentUsageStopped.UsageEnd = DateTime.Now;
                    string usageType = UsageTypes.Stopped.ToString();
                    var usageStoppedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
                    _currentUsageStopped.UsageTypeID = usageStoppedID;
                    context.Usages.Add(_currentUsageStopped);
                }
                if (_currentUsageLocked != null)
                {
                    string usageType = UsageTypes.Locked.ToString();
                    var usageLockedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
                    _currentUsageLocked.UsageTypeID = usageLockedID;
                    _currentUsageLocked.UsageEnd = DateTime.Now;
                    context.Usages.Add(_currentUsageLocked);
                }
                if (_currentUsageIdle != null)
                {
                    string usageType = UsageTypes.Idle.ToString();
                    var usageIdleID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
                    _currentUsageIdle.UsageTypeID = usageIdleID;
                    _currentUsageIdle.UsageEnd = DateTime.Now;
                    context.Usages.Add(_currentUsageIdle);
                }
                context.Entry(_currentUsageLogin).State = System.Data.Entity.EntityState.Modified;
                context.SaveChanges();
            }
            stopTask.Wait();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                DetachEventHandlers();
                if (_keyBoardHook != null) { _keyBoardHook.Dispose(); _keyBoardHook = null; }
                if (_winEvent != null) { _winEvent.Dispose(); _winEvent = null; }
                if (_screenshotTimer != null) { _screenshotTimer.Dispose(); _screenshotTimer = null; }
                if (_processKiller != null) { _processKiller.Dispose(); _processKiller = null; }
                if (_fileSystemWatcher != null) { _fileSystemWatcher.Dispose(); _fileSystemWatcher = null; }
                if (_windowCheckTimer != null) { _windowCheckTimer.Dispose(); _windowCheckTimer = null; }
                if (_idleMonitor != null) { _idleMonitor.Dispose(); _idleMonitor = null; }
            }
        }

        #endregion

        #region IComunicator

        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }

        #endregion
    }
}




//using System;
//using System.Data.Entity;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Task_Logger_Pro.Hooks;
//using Task_Logger_Pro.Models;
//using Task_Logger_Pro.MVVM;
//using System.Diagnostics;

//namespace Task_Logger_Pro.Logging
//{
//    public sealed class DataLogger : ObservableObject, IDisposable, ICommunicator
//    {
//        #region Fields

//        string _currentWindowTitle;

//        bool _takeScreenShots;
//        bool _notifyKeystrokes;
//        bool _disposed;
//        bool _enableIdle;
//        bool IsLogggingStopped;

//        LoggingStatus _loggingStatus;

//        Locker _logLocker = new Locker();

//        System.Timers.Timer _screenshotTimer;

//        System.Threading.Timer _windowCheckTimer;

//        //System.Threading.Timer _idleTimer;

//        IdleMonitor _idleMonitor;

//        FileWatcher _fileSystemWatcher;

//        KeyBoardHook _keyBoardHook;

//        WinEvent _winEvent;

//        ProcessKiller _processKiller;

//        Log _log;

//        Aplication _application;

//        Window _window;

//        Usage _usage;

//        Uzer _user;

//        #endregion

//        #region Properties

//        private Log Log
//        {
//            get
//            {
//                return _log;
//            }
//            set
//            {
//                if (_log != null && !_log.Finished)
//                    EndLog(_log);
//                if (_log != value)
//                    _log = value;
//            }

//        }

//        public double ScreenShotInterval
//        {
//            get { return _screenshotTimer.Interval; }
//            set { _screenshotTimer.Interval = value; }

//        }
//        public bool TakeScreenShots
//        {
//            get { return _takeScreenShots; }
//            set
//            {
//                _takeScreenShots = _screenshotTimer.Enabled = value;
//            }
//        }
//        public bool EnableIdle
//        {
//            get
//            {
//                return _enableIdle;
//            }
//            set
//            {
//                _enableIdle = value;
//                if (value)
//                    InitIdleMonitor();
//                else
//                    DisposeIdleMonitor();
//            }
//        }

//        public FileSystemWatcher FileSystemWatcher
//        {
//            get
//            {
//                if (_fileSystemWatcher == null) _fileSystemWatcher = new FileWatcher();
//                return _fileSystemWatcher;
//            }
//        }

//        public bool EnableFileWatcher
//        {
//            get { return _fileSystemWatcher == null ? false : _fileSystemWatcher.EnableRaisingEvents; }
//            set
//            {
//                _fileSystemWatcher.EnableRaisingEvents = value;
//                if (value) AttachFileWatcherEventHandlers();
//                else DetachFileWatcherEventHandlers();
//            }
//        }

//        public KeyBoardHook KeyBoardHook
//        {
//            get { return _keyBoardHook; }
//        }

//        public LoggingStatus LoggingStatus
//        {
//            get { return _loggingStatus; }
//            set
//            {
//                _loggingStatus = value;
//                PropertyChanging("LoggingStatus");
//                if (value == Task_Logger_Pro.LoggingStatus.Running)
//                {
//                    AttachEventHandlers();
//                    (App.Current as App).ChangeTheme();
//                }
//                else
//                {
//                    StopLogging(_log);
//                    Log = null;
//                    (App.Current as App).ChangeTheme();
//                }
//            }
//        }

//        public Mediator Mediator
//        {
//            get { return Mediator.Instance; }
//        }

//        #endregion

//        #region Constructor

//        public DataLogger()
//        {
//            CreateLoadUser();
//            _keyBoardHook = new KeyBoardHook();
//            _winEvent = new WinEvent();
//            _screenshotTimer = new System.Timers.Timer();
//            _screenshotTimer.AutoReset = true;
//            LoggingStatus = App.UzerSetting.LoggingEnabled.ConvertToLoggingStatus();
//            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
//            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<List<AppsToBlock>>(AppsToBlockChanging));
//            Globals.GetDBSize();
//        }

//        #endregion

//        #region Event Handlers

//        private void KeyDownEventHandler(object snder, KeyboardHookEventArgs e)
//        {
//            if (_log == null || _currentWindowTitle != _winEvent.GetActiveWindowName())
//                NewWindowEvent(_winEvent.GetWinEventArgs(), _log);

//            if (_log != null)
//            {
//                if (e.KeyCode == 8)
//                    _log.RemoveLastKeyLogItem();
//                else if (e.KeyCode == 0x0D)
//                    _log.AppendNewKeyLogLine();
//                else
//                    _log.AppendKeyLog(e.String);
//            }
//        }

//        private void KeyPressEventHandler(object sender, KeyboardHookEventArgs e)
//        {
//            if (_log == null || _currentWindowTitle != _winEvent.GetActiveWindowName())
//                NewWindowEvent(_winEvent.GetWinEventArgs(), _log);
//            if (_log != null)
//            {
//                if (e.KeyCode == 8) _log.AppendKeyLogRaw("Backspace"); //  backspace
//                else if (e.KeyCode == 9) _log.AppendKeyLogRaw("Tab"); //  tab
//                else if (e.KeyCode == 13) _log.AppendKeyLogRaw("Enter"); //  enter
//                else if (e.KeyCode == 16) _log.AppendKeyLogRaw("Shift"); //  shift
//                else if (e.KeyCode == 17) _log.AppendKeyLogRaw("Ctrl"); //  ctrl
//                else if (e.KeyCode == 18) _log.AppendKeyLogRaw("Alt"); //  alt
//                else if (e.KeyCode == 19) _log.AppendKeyLogRaw("Pause"); //  pause/break
//                else if (e.KeyCode == 20) _log.AppendKeyLogRaw("Caps lock"); //  caps lock
//                else if (e.KeyCode == 27) _log.AppendKeyLogRaw("Escape"); //  escape
//                else if (e.KeyCode == 33) _log.AppendKeyLogRaw("Page up"); // page up, to avoid displaying alternate character and confusing people
//                else if (e.KeyCode == 34) _log.AppendKeyLogRaw("Page down"); // page down
//                else if (e.KeyCode == 35) _log.AppendKeyLogRaw("End"); // end
//                else if (e.KeyCode == 36) _log.AppendKeyLogRaw("Home"); // home
//                else if (e.KeyCode == 37) _log.AppendKeyLogRaw("Left arrow"); // left arrow
//                else if (e.KeyCode == 38) _log.AppendKeyLogRaw("Up arrow"); // up arrow
//                else if (e.KeyCode == 39) _log.AppendKeyLogRaw("Right arrow"); // right arrow
//                else if (e.KeyCode == 40) _log.AppendKeyLogRaw("Down arrow"); // down arrow
//                else if (e.KeyCode == 45) _log.AppendKeyLogRaw("Insert"); // insert
//                else if (e.KeyCode == 46) _log.AppendKeyLogRaw("Delete"); // delete
//                else if (e.KeyCode == 91) _log.AppendKeyLogRaw("Left window"); // left window
//                else if (e.KeyCode == 92) _log.AppendKeyLogRaw("Right window"); // right window
//                else if (e.KeyCode == 93) _log.AppendKeyLogRaw("Select key"); // select key
//                else if (e.KeyCode == 96) _log.AppendKeyLogRaw("Numpad 0"); // numpad 0
//                else if (e.KeyCode == 97) _log.AppendKeyLogRaw("Numpad 1"); // numpad 1
//                else if (e.KeyCode == 98) _log.AppendKeyLogRaw("Numpad 2"); // numpad 2
//                else if (e.KeyCode == 99) _log.AppendKeyLogRaw("Numpad 3"); // numpad 3
//                else if (e.KeyCode == 100) _log.AppendKeyLogRaw("Numpad 4"); // numpad 4
//                else if (e.KeyCode == 101) _log.AppendKeyLogRaw("Numpad 5"); // numpad 5
//                else if (e.KeyCode == 102) _log.AppendKeyLogRaw("Numpad 6"); // numpad 6
//                else if (e.KeyCode == 103) _log.AppendKeyLogRaw("Numpad 7"); // numpad 7
//                else if (e.KeyCode == 104) _log.AppendKeyLogRaw("Numpad 8"); // numpad 8
//                else if (e.KeyCode == 105) _log.AppendKeyLogRaw("Numpad 9"); // numpad 9
//                else if (e.KeyCode == 106) _log.AppendKeyLogRaw("Multiply"); // multiply
//                else if (e.KeyCode == 107) _log.AppendKeyLogRaw("Add"); // add
//                else if (e.KeyCode == 109) _log.AppendKeyLogRaw("Subtract"); // subtract
//                else if (e.KeyCode == 110) _log.AppendKeyLogRaw("Decimal point"); // decimal point
//                else if (e.KeyCode == 111) _log.AppendKeyLogRaw("Divide"); // divide
//                else if (e.KeyCode == 112) _log.AppendKeyLogRaw("F1"); // F1
//                else if (e.KeyCode == 113) _log.AppendKeyLogRaw("F2"); // F2
//                else if (e.KeyCode == 114) _log.AppendKeyLogRaw("F3"); // F3
//                else if (e.KeyCode == 115) _log.AppendKeyLogRaw("F4"); // F4
//                else if (e.KeyCode == 116) _log.AppendKeyLogRaw("F5"); // F5
//                else if (e.KeyCode == 117) _log.AppendKeyLogRaw("F6"); // F6
//                else if (e.KeyCode == 118) _log.AppendKeyLogRaw("F7"); // F7
//                else if (e.KeyCode == 119) _log.AppendKeyLogRaw("F8"); // F8
//                else if (e.KeyCode == 120) _log.AppendKeyLogRaw("F9"); // F9
//                else if (e.KeyCode == 121) _log.AppendKeyLogRaw("F10"); // F10
//                else if (e.KeyCode == 122) _log.AppendKeyLogRaw("F11"); // F11
//                else if (e.KeyCode == 123) _log.AppendKeyLogRaw("F12"); // F12
//                else if (e.KeyCode == 144) _log.AppendKeyLogRaw("Num lock"); // num lock
//                else if (e.KeyCode == 145) _log.AppendKeyLogRaw("Scroll lock"); // scroll lock
//                else _log.AppendKeyLogRaw(e.KeyName);

//                _notifyKeystrokes = true;
//            }
//        }

//        private void TimerElapsedEventHandler(object sender, EventArgs e)
//        {
//            AddScreenShot();
//        }

//        private void ActiveWindowChangedEventHandler(object sender, WinEventArgs e)
//        {
//            NewWindowEvent(e, _log);
//        }

//        private void CheckWindowTitle()
//        {
//            if (_currentWindowTitle != _winEvent.GetActiveWindowName())
//                NewWindowEvent(_winEvent.GetWinEventArgs(), _log);
//        }

//        private void watcher_Renamed(object sender, RenamedEventArgs e)
//        {
//            if (_fileSystemWatcher != null)
//            {
//                Models.FileLog fileLog = new Models.FileLog(e.OldFullPath, e.ChangeType.ToString(), e.FullPath, _user.UserID);
//                using (var context = new AppsEntities1())
//                {
//                    context.FileLogs.Add(fileLog);
//                    context.SaveChanges();
//                }
//            }
//        }

//        private void watcher_Changed(object sender, FileSystemEventArgs e)
//        {
//            if (_fileSystemWatcher != null)
//            {
//                Models.FileLog fileLog = new Models.FileLog(e.FullPath, e.ChangeType.ToString(), _user.UserID);
//                using (var context = new AppsEntities1())
//                {
//                    context.FileLogs.Add(fileLog);
//                    context.SaveChanges();
//                }
//            }
//        }


//        private void _processKiller_ProcessKilledEvent(object sender, ProcessKilledEventArgs e)
//        {
//            using (var context = new AppsEntities1())
//            {
//                BlockedApp blockedApp = new BlockedApp() { Date = DateTime.Now, ApplicationID = e.Aplication.ApplicationID, UserID = Globals.UserID };
//                context.BlockedApps.Add(blockedApp);
//                context.SaveChanges();
//            }
//        }

//        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
//        {
//            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
//            {
//                using (var context = new AppsEntities1())
//                {
//                    string usageType = UsageTypes.Locked.ToString();
//                    Usage usage = new Usage(Globals.UserID, context.UsageTypes.First(u => u.UType == usageType).UsageTypeID);
//                    context.Usages.Add(usage);
//                    context.SaveChanges();
//                }
//                StopLogging(_log);
//                Log = null;
//                DisposeIdleMonitor();
//            }
//            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
//            {
//                using (var context = new AppsEntities1())
//                {
//                    string usageType = UsageTypes.Locked.ToString();

//                    var usage = (from u in context.Users.AsNoTracking()
//                                 join l in context.Usages.AsNoTracking() on u.UserID equals l.UserID
//                                 where l.UsageEnd == null
//                                 && l.UsageType.UType == usageType
//                                 orderby l.UsageStart descending
//                                 select l).FirstOrDefault();

//                    if (usage != null)
//                    {
//                        usage.UsageEnd = DateTime.Now;
//                        context.Entry(usage).State = EntityState.Modified;
//                        context.SaveChanges();
//                    }
//                }
//                AttachEventHandlers();
//                if (EnableIdle)
//                    InitIdleMonitor();
//            }
//        }


//        #endregion

//        #region Class Methods


//        private void InitIdleMonitor()
//        {
//            if (_idleMonitor == null)
//                _idleMonitor = new IdleMonitor();
//            _idleMonitor.IdleEntered += IdleEntered;
//            _idleMonitor.IdleStoped += IdleStoped;
//        }

//        private void DisposeIdleMonitor()
//        {
//            _idleMonitor.IdleEntered -= IdleEntered;
//            _idleMonitor.IdleStoped -= IdleStoped;
//            _idleMonitor.Dispose();
//            _idleMonitor = null;
//        }
//        //private void InitIdleTimer()
//        //{
//        //    if (_idleTimer == null)
//        //        _idleTimer = new System.Threading.Timer(CheckIdleState, null, 120000, 1000);
//        //    else
//        //        _idleTimer.Change(120000, 1000);
//        //    Console.WriteLine("Idle timer init, " +_idleTimer.ToString());
//        //}


//        //private void StopIdleTimer()
//        //{
//        //    if (_idleTimer == null)
//        //        return;
//        //    _idleTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
//        //}

//        private void IdleEntered(object sender, EventArgs e)
//        {
//            StopLogging(Log);
//            Log = null;
//            string usageType = UsageTypes.Idle.ToString();
//            using (var context = new AppsEntities1())
//            {
//                Usage usage = new Usage(Globals.UserID, context.UsageTypes.First(u => u.UType == usageType).UsageTypeID);
//                context.Usages.Add(usage);
//                context.SaveChanges();
//            }
//        }

//        //private Task AddNewUsageAsync(string usageType)
//        //{
//        //    return Task.Run(() =>
//        //    {
//        //        using (var context = new AppsEntities1())
//        //        {
//        //            Usage usage = new Usage(Globals.UserID, context.UsageTypes.First(u => u.UType == usageType).UsageTypeID);
//        //            context.Usages.Add(usage);
//        //            context.SaveChanges();
//        //        }
//        //    });
//        //}

//        private void IdleStoped(object sender, EventArgs e)
//        {
//            AttachEventHandlers();

//            using (var context = new AppsEntities1())
//            {
//                string usageType = UsageTypes.Idle.ToString();

//                var usage = (from u in context.Users.AsNoTracking()
//                             join l in context.Usages.AsNoTracking() on u.UserID equals l.UserID
//                             where l.UsageEnd == null
//                             && l.UsageType.UType == usageType
//                             orderby l.UsageStart descending
//                             select l).FirstOrDefault();

//                if (usage != null)
//                {
//                    usage.UsageEnd = DateTime.Now;
//                    context.Entry(usage).State = EntityState.Modified;
//                    context.SaveChanges();
//                }
//            }
//        }

//        private void AppsToBlockChanging(List<AppsToBlock> appsToBlockList)
//        {
//            if (appsToBlockList.Count == 0 && _processKiller != null)
//            {
//                _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;
//                _processKiller.Dispose();
//                _processKiller = null;
//                return;
//            }
//            else if (_processKiller == null)
//            {
//                _processKiller = new ProcessKiller();
//                _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
//            }
//            _processKiller.AppsToBlockList = appsToBlockList;
//        }

//        private void CreateLoadUser()
//        {
//            using (var context = new AppsEntities1())
//            {
//                if (!context.Users.CheckIfUserExists(Environment.UserName))
//                {
//                    _user = new Uzer() { Name = Environment.UserName };
//                    context.Users.Add(_user);
//                }
//                else
//                    _user = context.Users.FirstOrDefault(u => u.Name == Environment.UserName);
//                string uType = UsageTypes.Login.ToString();
//                _usage = new Usage() { UsageStart = DateTime.Now, UserID = _user.UserID, UsageTypeID = context.UsageTypes.First(u => u.UType == uType).UsageTypeID };

//                context.Usages.Add(_usage);
//                context.SaveChanges();

//                Globals.Initialize(_user, context);
//            }
//        }

//        private void StopLogging(Log log)
//        {
//            Log logTemp = log;
//            if (logTemp != null)
//            {
//                using (var context = new AppsEntities1())
//                {
//                    EndLog(logTemp, context);
//                }
//            }
//            if (_screenshotTimer.Enabled)
//                _screenshotTimer.Enabled = false;
//            DetachEventHandlers();
//            _currentWindowTitle = string.Empty;
//            _windowCheckTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
//        }

//        internal void DetachEventHandlers()
//        {
//            if (_keyBoardHook != null)
//            {
//                _keyBoardHook.KeyPress -= KeyPressEventHandler;
//                _keyBoardHook.KeyDown -= KeyDownEventHandler;
//            }

//            if (_winEvent != null)
//                _winEvent.ActiveWindowChanged -= ActiveWindowChangedEventHandler;

//            if (_screenshotTimer != null)
//                _screenshotTimer.Elapsed -= TimerElapsedEventHandler;
//            DetachFileWatcherEventHandlers();

//            if (_processKiller != null)
//                _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;

//            //if (_idleMonitor != null)
//            //{
//            //    _idleMonitor.IdleEntered -= IdleEntered;
//            //    _idleMonitor.IdleStoped -= IdleStoped;
//            //}

//        }

//        private void DetachFileWatcherEventHandlers()
//        {
//            if (_fileSystemWatcher != null)
//            {
//                _fileSystemWatcher.Created -= watcher_Changed;
//                _fileSystemWatcher.Deleted -= watcher_Changed;
//                _fileSystemWatcher.Renamed -= watcher_Renamed;

//                _fileSystemWatcher.Created -= watcher_Changed;
//                _fileSystemWatcher.Deleted -= watcher_Changed;
//                _fileSystemWatcher.Renamed -= watcher_Renamed;
//            }
//        }

//        private void AttachEventHandlers()
//        {

//            if (_windowCheckTimer == null)
//                _windowCheckTimer = new System.Threading.Timer((o) => App.Current.Dispatcher.Invoke(() => CheckWindowTitle()), null, 2 * 1000, 2 * 1000);
//            else
//                _windowCheckTimer.Change(2 * 1000, 2 * 1000);
//            _keyBoardHook.KeyPress += KeyPressEventHandler;
//            _keyBoardHook.KeyDown += KeyDownEventHandler;
//            _winEvent.ActiveWindowChanged += ActiveWindowChangedEventHandler;
//            if (_takeScreenShots)
//                _screenshotTimer.Enabled = true;
//            _screenshotTimer.Elapsed += TimerElapsedEventHandler;
//            AttachFileWatcherEventHandlers();
//            CheckWindowTitle();
//            CheckBlockedApps();
//        }

//        private void CheckBlockedApps()
//        {
//            using (var context = new AppsEntities1())
//            {
//                var appsToBlock = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Count();

//                if (appsToBlock > 0)
//                {
//                    if (_processKiller == null)
//                        _processKiller = new ProcessKiller();
//                    _processKiller.AppsToBlockList = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Include(a => a.Application).ToList();
//                    _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;
//                    _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
//                }
//            }
//        }

//        private void AttachFileWatcherEventHandlers()
//        {
//            if (_fileSystemWatcher != null)
//            {
//                _fileSystemWatcher.Created += watcher_Changed;
//                _fileSystemWatcher.Deleted += watcher_Changed;
//                _fileSystemWatcher.Renamed += watcher_Renamed;
//            }
//        }

//        private void AddScreenShot()
//        {
//            if (_currentWindowTitle != _winEvent.GetActiveWindowName())
//                NewWindowEvent(_winEvent.GetWinEventArgs(), Log);
//            Log log = Log;
//            Screenshot screenshot;

//            if (log == null)
//                return;
//            screenshot = Screenshots.GetScreenshot();
//            if (screenshot == null)
//                return;
//            screenshot.LogID = log.LogID;

//            using (var context = new AppsEntities1())
//            {
//                context.Screenshots.Add(screenshot);
//                context.SaveChanges();
//            }

//            Mediator.NotifyColleagues(MediatorMessages.ScreenshotAdded, new object());
//        }

//        #endregion

//        #region Logging Methods

//        private void NewWindowEvent(WinEventArgs e, Log log)
//        {
//            if (IsLogggingStopped)
//                return;

//            Log logTemp = log;

//            Debug.Assert(!string.IsNullOrEmpty(e.ProcessInfo.ProcessName), string.Format("Process name is empty, window = {0}", e.WindowTitle));

//            using (var context = new AppsEntities1())
//            {
//                if (string.IsNullOrEmpty(e.ProcessInfo.ProcessName))
//                {
//                    if (logTemp != null)
//                        EndLog(logTemp, context);
//                    Log = null;
//                    return;
//                }

//                bool newApp = false;

//                if (logTemp != null)
//                    EndLog(logTemp, context);


//                _application = (from a in context.Applications
//                                where a.UserID == Globals.UserID
//                                && a.Name == e.ProcessInfo.ProcessName
//                                select a).FirstOrDefault();

//                if (_application == null)
//                {
//                    _application = new Aplication(e.ProcessInfo.ProcessName,
//                                                 e.ProcessInfo.ProcessFileName,
//                                                 e.ProcessInfo.ProcessVersion,
//                                                 e.ProcessInfo.ProcessDescription,
//                                                 e.ProcessInfo.ProcessComments,
//                                                 e.ProcessInfo.ProcessCompany,
//                                                 e.ProcessInfo.ProcessRealName) { UserID = Globals.UserID };
//                    newApp = true;
//                    context.Applications.Add(_application);
//                }

//                _window = (from w in context.Windows
//                           where w.Title == e.WindowTitle
//                           && w.ApplicationID == _application.ApplicationID
//                           select w).FirstOrDefault();

//                if (_window == null)
//                {
//                    _window = new Window() { Title = e.WindowTitle, ApplicationID = _application.ApplicationID };
//                    context.Windows.Add(_window);
//                }



//                _currentWindowTitle = e.WindowTitle;

//                Log = new Log(_window.WindowID);

//                lock (_logLocker)
//                {
//                    context.Logs.Add(Log);
//                    context.SaveChanges();
//                    context.Entry(Log).State = EntityState.Detached;
//                }

//                if (newApp)
//                {
//                    context.Entry(_application).Collection(a => a.Windows).Load();
//                    Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, _application);
//                }
//                if (_notifyKeystrokes)
//                {
//                    Mediator.NotifyColleagues(MediatorMessages.KeystrokeAdded, new object());
//                    _notifyKeystrokes = false;
//                }
//            }
//        }

//        private void EndLog(Log log, AppsEntities1 context)
//        {
//            Log logTemp = log;
//            if (log.LogID == 0)
//                Debug.Fail("LogID = 0!");
//            logTemp.Finish();
//            lock (_logLocker)
//            {
//                if (!context.Logs.Local.Any(l => l.LogID == logTemp.LogID))
//                {
//                    context.Logs.Attach(logTemp);
//                }
//                if (context.Logs.FirstOrDefault(l => l.LogID == logTemp.LogID) == null)
//                    context.Logs.Add(logTemp);
//                else
//                    context.Entry(logTemp).State = System.Data.Entity.EntityState.Modified;
//                try
//                {
//                    context.SaveChanges();
//                }
//                catch (System.Data.Entity.Core.OptimisticConcurrencyException)
//                {
//                    context.Entry<Log>(logTemp).Reload();
//                    context.SaveChanges();
//                }
//                finally
//                {
//                    context.Entry(logTemp).State = EntityState.Detached;
//                }
//            }
//        }

//        private void EndLog(Models.Log log)
//        {
//            using (var context = new AppsEntities1())
//            {
//                Log logTemp = log;
//                if (log.LogID == 0)
//                    Debug.Fail("LogID = 0!");
//                logTemp.Finish();
//                lock (_logLocker)
//                {
//                    if (!context.Logs.Local.Any(l => l.LogID == logTemp.LogID))
//                    {
//                        context.Logs.Attach(logTemp);
//                    }
//                    if (context.Logs.FirstOrDefault(l => l.LogID == logTemp.LogID) == null)
//                        context.Logs.Add(logTemp);
//                    else
//                        context.Entry(logTemp).State = System.Data.Entity.EntityState.Modified;
//                    try
//                    {
//                        context.SaveChanges();
//                    }
//                    catch (System.Data.Entity.Core.OptimisticConcurrencyException)
//                    {
//                        context.Entry<Log>(logTemp).Reload();
//                        context.SaveChanges();
//                    }
//                    finally
//                    {
//                        context.Entry(logTemp).State = EntityState.Detached;
//                    }
//                }
//            }
//        }

//        internal void FinishLogging()
//        {
//            IsLogggingStopped = true;
//            using (var context = new AppsEntities1())
//            {
//                if (_log != null && !_log.Finished)
//                {
//                    _log.Finish();
//                    context.Entry(_log).State = System.Data.Entity.EntityState.Modified;
//                }
//                if (_usage != null)
//                {
//                    _usage.UsageEnd = DateTime.Now;
//                    context.Entry(_usage).State = System.Data.Entity.EntityState.Modified;
//                }

//                string ignore = UsageTypes.Login.ToString();

//                var unfinished = (from u in context.Users
//                                  join l in context.Usages on u.UserID equals l.UserID
//                                  where l.UserID == Globals.UserID
//                                  && l.UsageEnd == null
//                                  && l.UsageType.UType != ignore
//                                  select l).ToList();

//                foreach (var usage in unfinished)
//                {
//                    usage.UsageEnd = usage.UsageStart;
//                    context.Entry(usage).State = EntityState.Modified;
//                }

//                var logNotEnded = context.Logs.Where(l => l.DateEnded == null).Include(l => l.Window).ToList();
//                foreach (var log in logNotEnded)
//                {
//                    log.DateEnded = log.DateCreated;
//                    context.Entry(log).State = EntityState.Modified;
//                }

//                context.SaveChanges();
//            }
//        }

//        #endregion

//        #region IDisposable Members

//        private void Dispose(bool disposing)
//        {
//            System.Diagnostics.Debug.WriteLine("Disposing " + this.GetType().Name + " " + this.GetType().FullName);

//            if (!_disposed)
//            {
//                DetachEventHandlers();
//                if (_keyBoardHook != null) { _keyBoardHook.Dispose(); _keyBoardHook = null; }
//                if (_winEvent != null) { _winEvent.Dispose(); _winEvent = null; }
//                if (_screenshotTimer != null) { _screenshotTimer.Dispose(); _screenshotTimer = null; }
//                if (_processKiller != null) { _processKiller.Dispose(); _processKiller = null; }
//                if (_fileSystemWatcher != null) { _fileSystemWatcher.Dispose(); _fileSystemWatcher = null; }
//                if (_windowCheckTimer != null) { _windowCheckTimer.Dispose(); _windowCheckTimer = null; }
//                // if (_idleTimer != null) { _idleTimer.Dispose(); _idleTimer = null; }
//                if (_idleMonitor != null) { _idleMonitor.Dispose(); _idleMonitor = null; }
//                _disposed = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        #endregion

//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Task_Logger_Pro.Hooks;
//using Task_Logger_Pro.Models;
//using Task_Logger_Pro.MVVM;

//namespace Task_Logger_Pro.Logging
//{
//    public sealed class NewDataLogger : ObservableObject, IDisposable, ICommunicator
//    {

//        bool _disposed = false;
//        bool _stopLogging = false;
//        bool _enableIdle;
//        bool _enableKeyboardHook;
//        bool _takeScreenShots;
//        bool _enableFileWatcher;

//        double _screenshotInterval;

//        string _currentWindowTitle;

//        LoggingStatus _currentLoggingStatus;

//        Locker _currentLogLocker = new Locker();

//        System.Timers.Timer _screenshotTimer;

//        System.Threading.Timer _windowCheckTimer;

//        IdleMonitor _idleMonitor;

//        FileWatcher _fileSystemWatcher;

//        KeyBoardHook _keyBoardHook;

//        WinEvent _winEvent;

//        ProcessKiller _processKiller;

//        Log _currentLog;

//        Usage _currentUsageLocked;
//        Usage _currentUsageIdle;
//        Usage _currentUsageLogin;

//        Uzer _currentUser;

//        private Log CurrentLog
//        {
//            get
//            {
//                return _currentLog;
//            }
//            set
//            {
//                if (_currentLog != null && !_currentLog.Finished)
//                    EndSaveLog(_currentLog).Wait();
//                if (_currentLog != value)
//                    _currentLog = value;
//            }

//        }

//        public double ScreenShotInterval
//        {
//            get
//            {
//                return _screenshotInterval;
//            }
//            set
//            {
//                _screenshotInterval = value;
//                if (_screenshotTimer != null)
//                    _screenshotTimer.Interval = value;
//            }

//        }
//        public bool TakeScreenShots
//        {
//            get
//            {
//                return _takeScreenShots;
//            }
//            set
//            {
//                if (value != _takeScreenShots)
//                {
//                    _takeScreenShots = _screenshotTimer.Enabled = value;
//                    if (_takeScreenShots)
//                        InitScreenshotTimer();
//                    else
//                        DisposeScreenshotTimer();
//                }
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

//        public bool EnableKeyboardHook
//        {
//            get
//            {
//                return _enableKeyboardHook;
//            }
//            set
//            {
//                if (value != _enableKeyboardHook)
//                {
//                    _enableKeyboardHook = value;
//                    if (_enableKeyboardHook)
//                        InitKeyboardHook();
//                    else
//                        DisposeKeyboardHook();
//                }
//            }
//        }

//        public FileSystemWatcher FileSystemWatcher
//        {
//            get
//            {
//                if (_fileSystemWatcher == null)
//                    _fileSystemWatcher = new FileWatcher();
//                return _fileSystemWatcher;
//            }
//        }

//        public bool EnableFileWatcher
//        {
//            get
//            {
//                return _enableFileWatcher;
//            }
//            set
//            {
//                if (value != _enableFileWatcher)
//                {
//                    _enableFileWatcher = value;
//                    if (_enableFileWatcher)
//                        InitFileWatcher();
//                    else
//                        DisposeFileWatcher();
//                }
//            }
//        }

//        public KeyBoardHook KeyBoardHook
//        {
//            get
//            {
//                return _keyBoardHook;
//            }
//        }

//        public LoggingStatus LoggingStatus
//        {
//            get
//            {
//                return _currentLoggingStatus;
//            }
//            set
//            {
//                _currentLoggingStatus = value;
//                PropertyChanging("LoggingStatus");
//                if (value == Task_Logger_Pro.LoggingStatus.Running)
//                {
//                    if (_stopLogging)
//                        ResumeLogging();
//                    (App.Current as App).ChangeTheme();
//                }
//                else
//                {
//                    if (!_stopLogging)
//                        StopLogging(_currentLog).Wait();
//                    (App.Current as App).ChangeTheme();
//                }
//            }
//        }

//        public NewDataLogger(UzerSetting settings)
//        {
//            CreateLoadUser();
//            _winEvent = new WinEvent();
//            _windowCheckTimer = new System.Threading.Timer((o) => App.Current.Dispatcher.Invoke(CheckWindowTitle), null, 1000, 1000);
//            this.EnableIdle = settings.EnableIdle;
//            this.EnableKeyboardHook = settings.EnableKeylogger;
//            this.TakeScreenShots = settings.TakeScreenshots;
//            this.ScreenShotInterval = settings.TimerInterval;
//            LoggingStatus = settings.LoggingEnabled.ConvertToLoggingStatus();
//            //AttachEventHandlers();
//            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
//            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<List<AppsToBlock>>(AppsToBlockChanging));
//            Globals.GetDBSize();
//        }

//        private void InitKeyboardHook()
//        {
//            if (_keyBoardHook != null)
//                return;
//            _keyBoardHook = new KeyBoardHook();
//            _keyBoardHook.KeyDown += KeyDownEventHandler;
//            _keyBoardHook.KeyPress += KeyPressEventHandler;
//        }

//        private void DisposeKeyboardHook()
//        {
//            if (_keyBoardHook == null)
//                return;
//            _keyBoardHook.KeyDown -= KeyDownEventHandler;
//            _keyBoardHook.KeyPress -= KeyPressEventHandler;
//            _keyBoardHook.Dispose();
//            _keyBoardHook = null;
//        }

//        private void InitScreenshotTimer()
//        {
//            if (_screenshotTimer != null)
//                return;
//            _screenshotTimer = new System.Timers.Timer();
//            _screenshotTimer.AutoReset = true;
//            _screenshotTimer.Enabled = true;
//            _screenshotTimer.Elapsed += TimerElapsedEventHandler;
//        }

//        private void DisposeScreenshotTimer()
//        {
//            if (_screenshotTimer == null)
//                return;
//            _screenshotTimer.Enabled = false;
//            _screenshotTimer.Elapsed -= TimerElapsedEventHandler;
//            _screenshotTimer.Dispose();
//            _screenshotTimer = null;
//        }

//        private void InitFileWatcher()
//        {
//            if (_fileSystemWatcher != null)
//                return;
//            _fileSystemWatcher = new FileWatcher();
//            _fileSystemWatcher.Created += watcher_Changed;
//            _fileSystemWatcher.Deleted += watcher_Changed;
//            _fileSystemWatcher.Renamed += watcher_Renamed;
//            _fileSystemWatcher.EnableRaisingEvents = true;
//        }

//        private void DisposeFileWatcher()
//        {
//            if (_fileSystemWatcher == null)
//                return;
//            _fileSystemWatcher.EnableRaisingEvents = false;
//            _fileSystemWatcher.Created += watcher_Changed;
//            _fileSystemWatcher.Deleted += watcher_Changed;
//            _fileSystemWatcher.Renamed += watcher_Renamed;
//            _fileSystemWatcher.Dispose();
//            _fileSystemWatcher = null;
//        }

//        #region Event Handlers

//        private void KeyDownEventHandler(object snder, KeyboardHookEventArgs e)
//        {
//            if (_stopLogging)
//                return;

//            if (_currentLog == null || _currentWindowTitle != _winEvent.GetActiveWindowName())
//                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());

//            if (_currentLog != null)
//            {
//                if (e.KeyCode == 8)
//                    _currentLog.RemoveLastKeyLogItem();
//                else if (e.KeyCode == 0x0D)
//                    _currentLog.AppendNewKeyLogLine();
//                else
//                    _currentLog.AppendKeyLog(e.String);
//            }
//        }

//        private void KeyPressEventHandler(object sender, KeyboardHookEventArgs e)
//        {
//            if (_stopLogging)
//                return;

//            if (_currentLog == null || _currentWindowTitle != _winEvent.GetActiveWindowName())
//                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());

//            if (_currentLog != null)
//            {
//                if (e.KeyCode == 8) _currentLog.AppendKeyLogRaw("Backspace"); //  backspace
//                else if (e.KeyCode == 9) _currentLog.AppendKeyLogRaw("Tab"); //  tab
//                else if (e.KeyCode == 13) _currentLog.AppendKeyLogRaw("Enter"); //  enter
//                else if (e.KeyCode == 16) _currentLog.AppendKeyLogRaw("Shift"); //  shift
//                else if (e.KeyCode == 17) _currentLog.AppendKeyLogRaw("Ctrl"); //  ctrl
//                else if (e.KeyCode == 18) _currentLog.AppendKeyLogRaw("Alt"); //  alt
//                else if (e.KeyCode == 19) _currentLog.AppendKeyLogRaw("Pause"); //  pause/break
//                else if (e.KeyCode == 20) _currentLog.AppendKeyLogRaw("Caps lock"); //  caps lock
//                else if (e.KeyCode == 27) _currentLog.AppendKeyLogRaw("Escape"); //  escape
//                else if (e.KeyCode == 33) _currentLog.AppendKeyLogRaw("Page up"); // page up, to avoid displaying alternate character and confusing people
//                else if (e.KeyCode == 34) _currentLog.AppendKeyLogRaw("Page down"); // page down
//                else if (e.KeyCode == 35) _currentLog.AppendKeyLogRaw("End"); // end
//                else if (e.KeyCode == 36) _currentLog.AppendKeyLogRaw("Home"); // home
//                else if (e.KeyCode == 37) _currentLog.AppendKeyLogRaw("Left arrow"); // left arrow
//                else if (e.KeyCode == 38) _currentLog.AppendKeyLogRaw("Up arrow"); // up arrow
//                else if (e.KeyCode == 39) _currentLog.AppendKeyLogRaw("Right arrow"); // right arrow
//                else if (e.KeyCode == 40) _currentLog.AppendKeyLogRaw("Down arrow"); // down arrow
//                else if (e.KeyCode == 45) _currentLog.AppendKeyLogRaw("Insert"); // insert
//                else if (e.KeyCode == 46) _currentLog.AppendKeyLogRaw("Delete"); // delete
//                else if (e.KeyCode == 91) _currentLog.AppendKeyLogRaw("Left window"); // left window
//                else if (e.KeyCode == 92) _currentLog.AppendKeyLogRaw("Right window"); // right window
//                else if (e.KeyCode == 93) _currentLog.AppendKeyLogRaw("Select key"); // select key
//                else if (e.KeyCode == 96) _currentLog.AppendKeyLogRaw("Numpad 0"); // numpad 0
//                else if (e.KeyCode == 97) _currentLog.AppendKeyLogRaw("Numpad 1"); // numpad 1
//                else if (e.KeyCode == 98) _currentLog.AppendKeyLogRaw("Numpad 2"); // numpad 2
//                else if (e.KeyCode == 99) _currentLog.AppendKeyLogRaw("Numpad 3"); // numpad 3
//                else if (e.KeyCode == 100) _currentLog.AppendKeyLogRaw("Numpad 4"); // numpad 4
//                else if (e.KeyCode == 101) _currentLog.AppendKeyLogRaw("Numpad 5"); // numpad 5
//                else if (e.KeyCode == 102) _currentLog.AppendKeyLogRaw("Numpad 6"); // numpad 6
//                else if (e.KeyCode == 103) _currentLog.AppendKeyLogRaw("Numpad 7"); // numpad 7
//                else if (e.KeyCode == 104) _currentLog.AppendKeyLogRaw("Numpad 8"); // numpad 8
//                else if (e.KeyCode == 105) _currentLog.AppendKeyLogRaw("Numpad 9"); // numpad 9
//                else if (e.KeyCode == 106) _currentLog.AppendKeyLogRaw("Multiply"); // multiply
//                else if (e.KeyCode == 107) _currentLog.AppendKeyLogRaw("Add"); // add
//                else if (e.KeyCode == 109) _currentLog.AppendKeyLogRaw("Subtract"); // subtract
//                else if (e.KeyCode == 110) _currentLog.AppendKeyLogRaw("Decimal point"); // decimal point
//                else if (e.KeyCode == 111) _currentLog.AppendKeyLogRaw("Divide"); // divide
//                else if (e.KeyCode == 112) _currentLog.AppendKeyLogRaw("F1"); // F1
//                else if (e.KeyCode == 113) _currentLog.AppendKeyLogRaw("F2"); // F2
//                else if (e.KeyCode == 114) _currentLog.AppendKeyLogRaw("F3"); // F3
//                else if (e.KeyCode == 115) _currentLog.AppendKeyLogRaw("F4"); // F4
//                else if (e.KeyCode == 116) _currentLog.AppendKeyLogRaw("F5"); // F5
//                else if (e.KeyCode == 117) _currentLog.AppendKeyLogRaw("F6"); // F6
//                else if (e.KeyCode == 118) _currentLog.AppendKeyLogRaw("F7"); // F7
//                else if (e.KeyCode == 119) _currentLog.AppendKeyLogRaw("F8"); // F8
//                else if (e.KeyCode == 120) _currentLog.AppendKeyLogRaw("F9"); // F9
//                else if (e.KeyCode == 121) _currentLog.AppendKeyLogRaw("F10"); // F10
//                else if (e.KeyCode == 122) _currentLog.AppendKeyLogRaw("F11"); // F11
//                else if (e.KeyCode == 123) _currentLog.AppendKeyLogRaw("F12"); // F12
//                else if (e.KeyCode == 144) _currentLog.AppendKeyLogRaw("Num lock"); // num lock
//                else if (e.KeyCode == 145) _currentLog.AppendKeyLogRaw("Scroll lock"); // scroll lock
//                else _currentLog.AppendKeyLogRaw(e.KeyName);

//            }
//        }

//        private void TimerElapsedEventHandler(object sender, EventArgs e)
//        {
//            if (!_stopLogging)
//                AddScreenShot();
//        }

//        private void ActiveWindowChangedEventHandler(object sender, WinEventArgs e)
//        {
//            CurrentLog = NewWindowEvent(e);
//        }

//        private void CheckWindowTitle()
//        {
//            if (_stopLogging)
//                return;
//            if (_currentWindowTitle != _winEvent.GetActiveWindowName())
//                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());
//        }

//        private void watcher_Renamed(object sender, RenamedEventArgs e)
//        {
//            if (_fileSystemWatcher != null && !_stopLogging)
//            {
//                Models.FileLog fileLog = new Models.FileLog(e.OldFullPath, e.ChangeType.ToString(), e.FullPath, Globals.UserID);
//                using (var context = new AppsEntities1())
//                {
//                    context.FileLogs.Add(fileLog);
//                    context.SaveChanges();
//                }
//            }
//        }

//        private void watcher_Changed(object sender, FileSystemEventArgs e)
//        {
//            if (_fileSystemWatcher != null && !_stopLogging)
//            {
//                Models.FileLog fileLog = new Models.FileLog(e.FullPath, e.ChangeType.ToString(), Globals.UserID);
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

//        private async void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
//        {
//            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
//            {
//                Debug.Assert(_currentUsageLocked == null, "CURRENT USAGE LOCKED NOT NULL (FAILED TO CATCH UNLOCK EVENT)");
//                _currentUsageLocked = new Usage(Globals.UserID);
//                await StopLogging(_currentLog);
//            }
//            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
//            {
//                ResumeLogging();
//                using (var context = new AppsEntities1())
//                {
//                    if (_currentUsageLocked != null)
//                    {
//                        _currentUsageLocked.UsageEnd = DateTime.Now;
//                        string usageType = UsageTypes.Locked.ToString();
//                        var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
//                        _currentUsageLocked.UsageTypeID = usageLockedID;
//                        context.Usages.Add(_currentUsageLocked);
//                        await context.SaveChangesAsync();
//                        _currentUsageLocked = null;
//                    }
//                }
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

//        private async void IdleEntered(object sender, EventArgs e)
//        {
//            if (_stopLogging)
//                return;
//            Debug.Assert(_currentUsageIdle == null, "CURRENT USAGE IDLE NOT NULL");
//            _currentUsageIdle = new Usage(Globals.UserID);
//            await StopLogging(CurrentLog);
//        }

//        private void IdleStoped(object sender, EventArgs e)
//        {
//            ResumeLogging();
//            if (_currentUsageIdle != null)
//            {
//                _currentUsageIdle.UsageEnd = DateTime.Now;
//                using (var context = new AppsEntities1())
//                {
//                    string usageType = UsageTypes.Idle.ToString();
//                    var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageIdle.UsageTypeID = usageLockedID;
//                    context.Usages.Add(_currentUsageIdle);
//                }
//                _currentUsageIdle = null;
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
//                    _currentUser = new Uzer() { Name = Environment.UserName };
//                    context.Users.Add(_currentUser);
//                }
//                else
//                    _currentUser = context.Users.FirstOrDefault(u => u.Name == Environment.UserName);

//                string uType = UsageTypes.Login.ToString();
//                _currentUsageLogin = new Usage() { UsageStart = DateTime.Now, UserID = _currentUser.UserID, UsageTypeID = context.UsageTypes.First(u => u.UType == uType).UsageTypeID };

//                context.Usages.Add(_currentUsageLogin);
//                context.SaveChanges();
//            }
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

//            if (_idleMonitor != null)
//            {
//                _idleMonitor.IdleEntered -= IdleEntered;
//                _idleMonitor.IdleStoped -= IdleStoped;
//            }
//            Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
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
//            _keyBoardHook.KeyPress += KeyPressEventHandler;
//            _keyBoardHook.KeyDown += KeyDownEventHandler;
//            _winEvent.ActiveWindowChanged += ActiveWindowChangedEventHandler;
//            _screenshotTimer.Elapsed += TimerElapsedEventHandler;
//            AttachFileWatcherEventHandlers();
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
//                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());

//            Log log = CurrentLog;
//            if (log == null)
//                return;

//            Screenshot screenshot;

//            screenshot = Screenshots.GetScreenshot();
//            if (screenshot == null)
//                return;
//            log.Screenshots.Add(screenshot);
//        }

//        #endregion

//        #region Logging Methods

//        private Log NewWindowEvent(WinEventArgs e)
//        {
//            if (_stopLogging || string.IsNullOrEmpty(e.ProcessInfo.ProcessName))
//                return null;

//            using (var context = new AppsEntities1())
//            {
//                bool newApp = false;

//                Aplication app = (from a in context.Applications
//                                  where a.UserID == Globals.UserID
//                                  && a.Name == e.ProcessInfo.ProcessName
//                                  select a).FirstOrDefault();

//                if (app == null)
//                {
//                    app = new Aplication(e.ProcessInfo.ProcessName,
//                                                 e.ProcessInfo.ProcessFileName,
//                                                 e.ProcessInfo.ProcessVersion,
//                                                 e.ProcessInfo.ProcessDescription,
//                                                 e.ProcessInfo.ProcessComments,
//                                                 e.ProcessInfo.ProcessCompany,
//                                                 e.ProcessInfo.ProcessRealName) { UserID = Globals.UserID };
//                    newApp = true;
//                    app = context.Applications.Add(app);
//                }

//                Debug.Assert(app.ApplicationID != 0, "Application ID = 0 " + app.Name);
//                Window window = (from w in context.Windows
//                                 where w.Title == e.WindowTitle
//                                 && w.ApplicationID == app.ApplicationID
//                                 select w).FirstOrDefault();

//                if (window == null)
//                {
//                    window = new Window() { Title = e.WindowTitle, ApplicationID = app.ApplicationID };
//                    window = context.Windows.Add(window);
//                }

//                Debug.Assert(window.WindowID != 0, "Window ID = 0 " + window.Title);

//                context.SaveChangesAsync();
//                _currentWindowTitle = e.WindowTitle;

//                if (newApp)
//                {
//                    context.Entry(app).Collection(a => a.Windows).Load();
//                    Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, app);
//                }

//                return new Log(window.WindowID);

//            }
//        }

//        private async Task EndSaveLog(Log log)
//        {
//            if (!log.Finished)
//                log.Finish();

//            using (var context = new AppsEntities1())
//            {
//                context.Logs.Add(log);
//                try
//                {
//                    await context.SaveChangesAsync();
//                }
//                catch (System.Data.Entity.Core.OptimisticConcurrencyException)
//                {
//                    context.Entry<Log>(log).Reload();
//                    context.SaveChanges();
//                }
//                //finally
//                //{
//                //    context.Entry(log).State = EntityState.Detached;
//                //}
//            }
//        }

//        private async Task StopLogging(Log log)
//        {
//            _stopLogging = true;
//            Log logTemp = log;
//            if (logTemp != null)
//            {
//                await EndSaveLog(logTemp);
//            }
//            _currentWindowTitle = string.Empty;
//            _windowCheckTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
//            CurrentLog = null;
//        }

//        private void ResumeLogging()
//        {
//            _stopLogging = false;
//            _windowCheckTimer.Change(1000, 1000);
//        }

//        internal void FinishLogging()
//        {
//            Task stopTask = StopLogging(CurrentLog);
//            stopTask.Wait();
//            using (var context = new AppsEntities1())
//            {

//                _currentUsageLogin.UsageEnd = DateTime.Now;
//                context.Entry(_currentUsageLogin).State = System.Data.Entity.EntityState.Modified;


//                string ignore = UsageTypes.Login.ToString();

//                var unfinished = (from u in context.Users
//                                  join l in context.Usages on u.UserID equals l.UserID
//                                  where l.UserID == Globals.UserID
//                                  && l.UsageEnd == null
//                                  && l.UsageType.UType != ignore
//                                  select l).ToList();

//                foreach (var usage in unfinished)
//                {
//                    Debug.Fail("Unfinished usage, ID =" + usage.UsageID);
//                    usage.UsageEnd = usage.UsageStart;
//                    context.Entry(usage).State = EntityState.Modified;
//                }

//                var logNotEnded = context.Logs.Where(l => l.DateEnded == null).Include(l => l.Window).ToList();
//                foreach (var log in logNotEnded)
//                {
//                    Debug.Fail("Unfinished log, ID =" + log.LogID);
//                    log.DateEnded = log.DateCreated;
//                    context.Entry(log).State = EntityState.Modified;
//                }

//                context.SaveChanges();
//            }
//        }

//        #endregion


//        public void Dispose()
//        {
//            if (!_disposed)
//            {
//                _disposed = true;
//                DetachEventHandlers();
//                if (_keyBoardHook != null) { _keyBoardHook.Dispose(); _keyBoardHook = null; }
//                if (_winEvent != null) { _winEvent.Dispose(); _winEvent = null; }
//                if (_screenshotTimer != null) { _screenshotTimer.Dispose(); _screenshotTimer = null; }
//                if (_processKiller != null) { _processKiller.Dispose(); _processKiller = null; }
//                if (_fileSystemWatcher != null) { _fileSystemWatcher.Dispose(); _fileSystemWatcher = null; }
//                if (_windowCheckTimer != null) { _windowCheckTimer.Dispose(); _windowCheckTimer = null; }
//                if (_idleMonitor != null) { _idleMonitor.Dispose(); _idleMonitor = null; }
//            }
//        }

//        public Mediator Mediator
//        {
//            get { return Mediator.Instance; }
//        }
//    }
//}

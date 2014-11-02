//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//using AppsTracker.Hooks;
//using AppsTracker.MVVM;
//using AppsTracker.DAL;
//using AppsTracker.Models.EntityModels;
//using AppsTracker.DAL.Service;

//namespace AppsTracker.Logging
//{
//    public sealed class DataLogger : IDisposable, ICommunicator
//    {
//        #region Fields

//        bool _disposed = false;
//        bool _loggingStoped = false;
//        bool _enableIdle;
//        bool _enableKeyboardHook;
//        bool _takeScreenShots;

//        double _screenshotInterval = 2 * 60 * 1000;

//        string _currentWindowTitle;

//        LoggingStatus _currentLoggingStatus;

//        System.Timers.Timer _screenshotTimer;

//        System.Threading.Timer _windowCheckTimer;

//        IdleMonitor _idleMonitor;

//        IHook<KeyboardHookArgs> _keyboardHook;
//        IHook<WinHookArgs> _winEventHook;

//        KeyBoardHook _keyBoardHook;

//        WinHook _winEvent;

//        ProcessKiller _processKiller;

//        Log _currentLog;

//        Usage _currentUsageLocked;
//        Usage _currentUsageIdle;
//        Usage _currentUsageLogin;
//        Usage _currentUsageStopped;

//        IAppsService _service;

//        #endregion

//        #region Properties

//        public bool IsLogggingStopped
//        {
//            get
//            {
//                return _loggingStoped;
//            }
//            set
//            {
//                _loggingStoped = value;
//                if (_idleMonitor != null)
//                    _idleMonitor.Enabled = !value;
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
//                    _takeScreenShots = value;
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
//                if (_enableIdle)
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

//        internal KeyBoardHook KeyBoardHook
//        {
//            get
//            {
//                return _keyBoardHook;
//            }
//        }

//        private Log CurrentLog
//        {
//            get
//            {
//                return _currentLog;
//            }
//            set
//            {
//                if (_currentLog != null && !_currentLog.Finished)
//                    EndSaveLog(_currentLog);
//                if (_currentLog != value)
//                    _currentLog = value;
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
//                if (_currentLoggingStatus != value)
//                {
//                    _currentLoggingStatus = value;

//                    if (value == AppsTracker.LoggingStatus.Running)
//                    {
//                        if (IsLogggingStopped)
//                            ResumeLogging();
//                        LoggingResumed();
//                    }
//                    else
//                    {
//                        if (!IsLogggingStopped)
//                            StopLogging(_currentLog);
//                        LoggingStopped();

//                    }

//                    (App.Current as App).ChangeTheme();
//                }
//            }
//        }

//        #endregion

//        #region Constructor

//        public DataLogger(SettingsProxy settings)
//        {
//            _service = ServiceFactory.Instance.GetService<IAppsService>();

//            CreateLoadUser();

//            _currentLoggingStatus = settings.LoggingEnabled.ConvertToLoggingStatus();
//            IsLogggingStopped = !settings.LoggingEnabled;

//            this.EnableIdle = settings.EnableIdle;
//            this.EnableKeyboardHook = settings.EnableKeylogger;
//            this.TakeScreenShots = settings.TakeScreenshots;
//            this.ScreenShotInterval = settings.TimerInterval;

//            _winEvent = new WinHook();
//            _winEvent.ActiveWindowChanged += ActiveWindowChangedEventHandler;
//            _windowCheckTimer = new System.Threading.Timer((o) => App.Current.Dispatcher.Invoke(CheckWindowTitle), null, 500, 500);

//            CheckBlockedApps();

//            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

//            Mediator.Register(MediatorMessages.AppsToBlockChanged, new Action<List<AppsToBlock>>(AppsToBlockChanging));

//            Globals.GetDBSize();
//        }

//        #endregion

//        #region Init & Dispose Methods

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
//            _screenshotTimer.Interval = _screenshotInterval;
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

//        private void InitIdleMonitor()
//        {
//            if (_idleMonitor != null)
//                return;
//            _idleMonitor = new IdleMonitor();
//            _idleMonitor.IdleEntered += IdleEntered;
//            _idleMonitor.IdleStoped += IdleStoped;
//            _idleMonitor.Enabled = !_loggingStoped;
//        }

//        private void DisposeIdleMonitor()
//        {
//            if (_idleMonitor == null)
//                return;
//            _idleMonitor.IdleEntered -= IdleEntered;
//            _idleMonitor.IdleStoped -= IdleStoped;
//            _idleMonitor.Dispose();
//            _idleMonitor = null;
//        }

//        #endregion

//        #region Event Handlers

//        private void KeyDownEventHandler(object sender, KeyboardHookArgs e)
//        {
//            if (IsLogggingStopped)
//                return;

//            CheckWindowTitle();

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

//        private void KeyPressEventHandler(object sender, KeyboardHookArgs e)
//        {
//            if (IsLogggingStopped)
//                return;

//            CheckWindowTitle();

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
//            if (IsLogggingStopped)
//                return;

//            AddScreenShot();
//        }

//        private void ActiveWindowChangedEventHandler(object sender, WinHookArgs e)
//        {
//            if (IsLogggingStopped)
//                return;

//            CurrentLog = NewWindowEvent(e);
//        }

//        private void CheckWindowTitle()
//        {
//            if (IsLogggingStopped)
//                return;
//            if (_currentWindowTitle != _winEvent.GetActiveWindowName())
//                CurrentLog = NewWindowEvent(_winEvent.GetWinEventArgs());
//        }

//        private void _processKiller_ProcessKilledEvent(object sender, ProcessKilledEventArgs e)
//        {
//            _service.Add<BlockedApp>(new BlockedApp() { Date = DateTime.Now, ApplicationID = e.Aplication.ApplicationID, UserID = Globals.UserID });
//            //using (var context = new AppsEntities())
//            //{
//            //    BlockedApp blockedApp = new BlockedApp() { Date = DateTime.Now, ApplicationID = e.Aplication.ApplicationID, UserID = Globals.UserID };
//            //    context.BlockedApps.Add(blockedApp);
//            //    context.SaveChanges();
//            //}
//        }

//        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
//        {
//            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
//            {
//                StopLogging(CurrentLog);
//                Debug.Assert(_currentUsageLocked == null, "CURRENT USAGE LOCKED NOT NULL (FAILED TO CATCH UNLOCK EVENT)");
//                _currentUsageLocked = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
//                if (_currentUsageIdle != null)
//                {
//                    string usageType = UsageTypes.Idle.ToString();
//                    _currentUsageIdle.UsageEnd = DateTime.Now;
//                    SaveUsage(usageType, _currentUsageIdle);
//                    _currentUsageIdle = null;
//                }
//                StartStopIdleMonitor(false);
//            }
//            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
//            {
//                ResumeLogging();
//                StartStopIdleMonitor(true);
//                if (_currentUsageLocked != null)
//                {
//                    string usageType = UsageTypes.Locked.ToString();
//                    _currentUsageLocked.UsageEnd = DateTime.Now;
//                    SaveUsage(usageType, _currentUsageLocked);
//                    _currentUsageLocked = null;
//                }
//            }
//        }

//        private void StartStopIdleMonitor(bool enabled)
//        {
//            if (_idleMonitor == null)
//                return;
//            _idleMonitor.Enabled = enabled;
//        }

//        private void SaveUsage(string usageType, Usage usage)
//        {
//            var usageT = _service.GetSingle<UsageType>(t => t.UType == usageType);
//            if (usageT == null)
//                throw new InvalidOperationException(string.Concat("Can't load ", usageType));
//            var usageID = usageT.UsageTypeID;
//            usage.UsageTypeID = usageID;
//            _service.Add<Usage>(usage);

//            //using (var context = new AppsEntities())
//            //{
//            //    var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
//            //    usage.UsageTypeID = usageLockedID;
//            //    context.Usages.Add(usage);
//            //    context.SaveChanges();
//            //}
//        }
//        private void IdleEntered(object sender, EventArgs e)
//        {
//            if (IsLogggingStopped)
//                return;
//            Debug.Assert(_currentUsageIdle == null, "CURRENT USAGE IDLE NOT NULL");
//            _currentUsageIdle = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
//            StopLogging(CurrentLog);
//        }

//        private void IdleStoped(object sender, EventArgs e)
//        {
//            ResumeLogging();
//            if (_currentUsageIdle != null)
//            {
//                _currentUsageIdle.UsageEnd = DateTime.Now;
//                using (var context = new AppsEntities())
//                {
//                    string usageType = UsageTypes.Idle.ToString();
//                    var usageLockedID = context.UsageTypes.Where(t => t.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageIdle.UsageTypeID = usageLockedID;
//                    context.Usages.Add(_currentUsageIdle);
//                    context.SaveChanges();
//                }
//                _currentUsageIdle = null;
//            }
//        }

//        #endregion

//        #region Class Methods

//        private void LoggingStopped()
//        {
//            if (_currentUsageStopped == null)
//                _currentUsageStopped = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };

//            StartStopIdleMonitor(false);
//        }

//        private void LoggingResumed()
//        {
//            if (_currentUsageStopped != null)
//            {
//                _currentUsageStopped.UsageEnd = DateTime.Now;

//                using (var context = new AppsEntities())
//                {
//                    string usageType = UsageTypes.Stopped.ToString();
//                    var usageStoppedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageStopped.UsageTypeID = usageStoppedID;
//                    context.Usages.Add(_currentUsageStopped);
//                    context.SaveChanges();
//                }
//                _currentUsageStopped = null;
//            }

//            StartStopIdleMonitor(true);
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
//            else if (_processKiller == null && appsToBlockList.Count > 0)
//            {
//                _processKiller = new ProcessKiller(appsToBlockList);
//                _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
//                _processKiller.AppsToBlockList = appsToBlockList;
//            }
//            else if (_processKiller != null && appsToBlockList.Count > 0)
//                _processKiller.AppsToBlockList = appsToBlockList;
//        }

//        private void CreateLoadUser()
//        {
//            Uzer user;
//            using (var context = new AppsEntities())
//            {
//                if (!context.Users.Any(u => u.Name == Environment.UserName))
//                {
//                    user = new Uzer() { Name = Environment.UserName };
//                    context.Users.Add(user);
//                }
//                else
//                    user = context.Users.FirstOrDefault(u => u.Name == Environment.UserName);

//                string filterUsage = UsageTypes.Login.ToString();

//                if (context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == filterUsage).Count() > 0)
//                {
//                    var failedSaveUsage = context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == filterUsage).ToList();
//                    foreach (var usage in failedSaveUsage)
//                    {
//                        var lastLog = context.Logs.Where(l => l.UsageID == usage.UsageID).OrderByDescending(l => l.DateCreated).FirstOrDefault();
//                        var lastUsage = context.Usages.Where(u => u.SelfUsageID == usage.UsageID).OrderByDescending(u => u.UsageEnd).FirstOrDefault();

//                        DateTime lastLogDate = DateTime.MinValue;
//                        DateTime lastUsageDate = DateTime.MinValue;

//                        if (lastLog != null)
//                            lastLogDate = lastLog.DateEnded;

//                        if (lastUsage != null)
//                            lastUsageDate = lastUsage.UsageEnd;


//                        usage.UsageEnd = lastLogDate == lastUsageDate ? usage.UsageEnd : lastUsageDate > lastLogDate ? lastUsageDate : lastLogDate;
//                        usage.IsCurrent = false;
//                        context.Entry(usage).State = EntityState.Modified;
//                    }
//                }

//                _currentUsageLogin = new Usage(user.UserID) { UsageEnd = DateTime.Now, UsageTypeID = context.UsageTypes.First(u => u.UType == filterUsage).UsageTypeID, IsCurrent = true };

//                context.Usages.Add(_currentUsageLogin);
//                context.SaveChanges();
//                Globals.Initialize(user, _currentUsageLogin.UsageID, context);
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

//            if (_processKiller != null)
//                _processKiller.ProcessKilledEvent -= _processKiller_ProcessKilledEvent;

//            if (_idleMonitor != null)
//            {
//                _idleMonitor.IdleEntered -= IdleEntered;
//                _idleMonitor.IdleStoped -= IdleStoped;
//            }
//            Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
//        }

//        private void CheckBlockedApps()
//        {
//            using (var context = new AppsEntities())
//            {
//                var appsToBlock = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Count();

//                if (appsToBlock > 0)
//                {
//                    if (_processKiller == null)
//                        _processKiller = new ProcessKiller();
//                    _processKiller.AppsToBlockList = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Include(a => a.Application).ToList();
//                    _processKiller.ProcessKilledEvent += _processKiller_ProcessKilledEvent;
//                }
//            }
//        }

//        private async void AddScreenShot()
//        {
//            var dbSizeAsync = Globals.GetDBSizeAsync();
//            CheckWindowTitle();

//            Log log = CurrentLog;
//            Screenshot screenshot;

//            screenshot = Screenshots.GetScreenshot();
//            if (screenshot == null || log == null)
//                return;
//            log.Screenshots.Add(screenshot);

//            await dbSizeAsync.ConfigureAwait(true); //check the DB size, this methods fires an event if near the maximum allowed size
//        }

//        #endregion

//        #region Logging Methods

//        private Log NewWindowEvent(WinHookArgs e)
//        {
//            if (e.ProcessInfo == null || string.IsNullOrEmpty(e.ProcessInfo.ProcessName))
//                return null;
//            bool newAppAdded;
//            var log = _service.CreateNewLog(e.WindowTitle, Globals.UsageID, Globals.UserID, null, out newAppAdded);

//            //using (var context = new AppsEntities())
//            //{
//            //    bool newApp = false;

//            //    Aplication app = (from a in context.Applications
//            //                      where a.UserID == Globals.UserID
//            //                      && a.Name == e.ProcessInfo.ProcessName
//            //                      select a).FirstOrDefault();

//            //    if (app == null)
//            //    {
//            //        app = new Aplication(e.ProcessInfo.ProcessName,
//            //                                        e.ProcessInfo.ProcessFileName,
//            //                                        e.ProcessInfo.ProcessVersion,
//            //                                        e.ProcessInfo.ProcessDescription,
//            //                                        e.ProcessInfo.ProcessCompany,
//            //                                        e.ProcessInfo.ProcessRealName) { UserID = Globals.UserID };
//            //        newApp = true;
//            //        context.Applications.Add(app);
//            //        try
//            //        {
//            //            context.SaveChanges();
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            if (ex is System.Data.Entity.Validation.DbEntityValidationException)
//            //                foreach (var eve in (ex as System.Data.Entity.Validation.DbEntityValidationException).EntityValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                        eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                    foreach (var ve in eve.ValidationErrors)
//            //                    {
//            //                        Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                            ve.PropertyName, ve.ErrorMessage));
//            //                    }
//            //                }
//            //            else if (ex.InnerException != null && ex.InnerException is System.Data.Entity.Validation.DbEntityValidationException)
//            //            {
//            //                System.Data.Entity.Validation.DbEntityValidationException evex = ex.InnerException as System.Data.Entity.Validation.DbEntityValidationException;
//            //                foreach (var eve in evex.EntityValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                        eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                    foreach (var ve in eve.ValidationErrors)
//            //                    {
//            //                        Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                            ve.PropertyName, ve.ErrorMessage));
//            //                    }
//            //                }
//            //            }
//            //            throw;
//            //        }
//            //    }

//            //    Window window = (from w in context.Windows
//            //                     where w.Title == e.WindowTitle
//            //                     && w.ApplicationID == app.ApplicationID
//            //                     select w).FirstOrDefault();

//            //    if (window == null)
//            //    {
//            //        window = Window.CreateByTitleAndAppID(e.WindowTitle, app.ApplicationID);

//            //        context.Windows.Add(window);
//            //        try
//            //        {
//            //            context.SaveChanges();
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            if (ex is System.Data.Entity.Validation.DbEntityValidationException)
//            //                foreach (var eve in (ex as System.Data.Entity.Validation.DbEntityValidationException).EntityValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                        eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                    foreach (var ve in eve.ValidationErrors)
//            //                    {
//            //                        Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                            ve.PropertyName, ve.ErrorMessage));
//            //                    }
//            //                }
//            //            else if (ex.InnerException != null && ex.InnerException is System.Data.Entity.Validation.DbEntityValidationException)
//            //            {
//            //                System.Data.Entity.Validation.DbEntityValidationException evex = ex.InnerException as System.Data.Entity.Validation.DbEntityValidationException;
//            //                foreach (var eve in evex.EntityValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                        eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                    foreach (var ve in eve.ValidationErrors)
//            //                    {
//            //                        Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                            ve.PropertyName, ve.ErrorMessage));
//            //                    }
//            //                }
//            //            }
//            //            throw;
//            //        }
//            //    }

//            _currentWindowTitle = e.WindowTitle;

//            if (newAppAdded)
//            {
//                //context.Entry(app).Collection(a => a.Windows).Load();
//                var newApp = _service.GetSingle<Aplication>(a => a.Name == e.ProcessInfo.ProcessName, a => a.Windows);
//                Mediator.NotifyColleagues(MediatorMessages.ApplicationAdded, newApp);
//            }
//            return log;
//            //return new Log(window.WindowID, Globals.UsageID);
//        }


//        private void EndSaveLog(Log log)
//        {
//            if (!log.Finished)
//                log.Finish();
//            _service.Add<Log>(log);
//            //using (var context = new AppsEntities())
//            //{
//            //    context.Logs.Add(log);
//            //    try
//            //    {
//            //        context.SaveChanges();
//            //    }
//            //    catch (System.Data.Entity.Core.OptimisticConcurrencyException)
//            //    {
//            //        context.Entry<Log>(log).Reload();
//            //        context.SaveChanges();
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        if (ex is System.Data.Entity.Validation.DbEntityValidationException)
//            //            foreach (var eve in (ex as System.Data.Entity.Validation.DbEntityValidationException).EntityValidationErrors)
//            //            {
//            //                Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                    eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                foreach (var ve in eve.ValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                        ve.PropertyName, ve.ErrorMessage));
//            //                }
//            //            }
//            //        else if (ex.InnerException != null && ex.InnerException is System.Data.Entity.Validation.DbEntityValidationException)
//            //        {
//            //            System.Data.Entity.Validation.DbEntityValidationException evex = ex.InnerException as System.Data.Entity.Validation.DbEntityValidationException;
//            //            foreach (var eve in evex.EntityValidationErrors)
//            //            {
//            //                Exceptions.Logger.DumpDebug(string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
//            //                    eve.Entry.Entity.GetType().Name, eve.Entry.State));
//            //                foreach (var ve in eve.ValidationErrors)
//            //                {
//            //                    Exceptions.Logger.DumpDebug(string.Format("- Property: \"{0}\", Error: \"{1}\"",
//            //                        ve.PropertyName, ve.ErrorMessage));
//            //                }
//            //            }
//            //        }
//            //        throw;
//            //    }
//            //}
//        }

//        private void StopLogging(Log log)
//        {
//            IsLogggingStopped = true;
//            if (log != null)
//                EndSaveLog(log);

//            _currentWindowTitle = string.Empty;
//            _windowCheckTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
//            _currentLog = null;
//        }

//        private void ResumeLogging()
//        {
//            IsLogggingStopped = false;
//            _windowCheckTimer.Change(500, 500);
//            CheckWindowTitle();
//        }

//        internal void FinishLogging()
//        {
//            StopLogging(CurrentLog);
//            _currentUsageLogin.UsageEnd = DateTime.Now;
//            _currentUsageLogin.IsCurrent = false;

//            using (var context = new AppsEntities())
//            {
//                _currentUsageLogin.UsageEnd = DateTime.Now;
//                _currentUsageLogin.IsCurrent = false;
//                if (_currentUsageStopped != null)
//                {
//                    _currentUsageStopped.UsageEnd = DateTime.Now;
//                    string usageType = UsageTypes.Stopped.ToString();
//                    var usageStoppedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageStopped.UsageTypeID = usageStoppedID;
//                    context.Usages.Add(_currentUsageStopped);
//                }
//                if (_currentUsageLocked != null)
//                {
//                    //string usageType = UsageTypes.Locked.ToString();
//                    var usageLockedID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageLocked.UsageTypeID = usageLockedID;
//                    _currentUsageLocked.UsageEnd = DateTime.Now;
//                    context.Usages.Add(_currentUsageLocked);
//                }
//                if (_currentUsageIdle != null)
//                {
//                    string usageType = UsageTypes.Idle.ToString();
//                    var usageIdleID = context.UsageTypes.Where(u => u.UType == usageType).FirstOrDefault().UsageTypeID;
//                    _currentUsageIdle.UsageTypeID = usageIdleID;
//                    _currentUsageIdle.UsageEnd = DateTime.Now;
//                    context.Usages.Add(_currentUsageIdle);
//                }
//                context.Entry(_currentUsageLogin).State = System.Data.Entity.EntityState.Modified;
//                context.SaveChanges();
//            }
//        }

//        #endregion

//        #region IDisposable

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
//                if (_windowCheckTimer != null) { _windowCheckTimer.Dispose(); _windowCheckTimer = null; }
//                if (_idleMonitor != null) { _idleMonitor.Dispose(); _idleMonitor = null; }
//                if (_service != null) { _service.Dispose(); _service = null; }
//            }
//        }

//        #endregion

//        #region IComunicator

//        public IMediator Mediator
//        {
//            get { return MVVM.Mediator.Instance; }
//        }

//        #endregion
//    }
//}


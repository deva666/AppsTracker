using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using Microsoft.Win32;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.Logging;
using Task_Logger_Pro.Utils;


namespace Task_Logger_Pro
{
    #region Enums

    public enum LoggingStatus : byte
    {
        [Description("Logging in progress ...")]
        Running,
        [Description("Logging stopped")]
        Stopped
    }

    public enum UsageTypes : byte
    {
        Login,
        Idle,
        Locked,
        Stopped
    }

    #endregion

    public partial class App : Application, IDisposable
    {
        #region Fields

        bool _disposed;
        bool _userTriggerExit = false;
        static DataLogger _dataLogger;
        static SettingsProxy _uzerSetting;
        static Setting _settings;
        TrayIcon _trayIcon;
        EmailReport _emailReport;
        SettingsQueue _settingsQueue;

        #endregion

        internal static DataLogger DataLogger { get { return _dataLogger; } }
        internal static SettingsProxy UzerSetting { get { return _uzerSetting; } }
        internal static Setting Settings { get { return _settings; } }

        #region Constructor

        public App(ReadOnlyCollection<string> args)
        {
            InitializeComponent();

            bool autostart = false;
            foreach (var arg in args)
            {
                if (arg.ToUpper().Contains(Constants.CMD_ARGS_AUTOSTART)) autostart = true;
            }

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement)
                                                                , new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new PropertyMetadata() { DefaultValue = 40 });

            this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

            CreateSettingsAndUsageTypes();

            _uzerSetting = new SettingsProxy();
            _settingsQueue = new SettingsQueue();
            _settingsQueue.SaveSettings += _settingsQueue_SaveSettings;

            ChangeTheme();

            _uzerSetting.PropertyChanged += (s, e) =>
            {
                UpdateLogData(e.PropertyName);
                _settingsQueue.Add(_uzerSetting);
                //SaveSettings(e.PropertyName);
            };

            Task deleteOldLogsTask = null;

            if (_uzerSetting.DeleteOldLogs)
                deleteOldLogsTask = DeleteOldLogsAsync();

            if (CheckTrialExpiration())
            {
                MessageWindow messageWindow = new MessageWindow("The trial period has expired." + Environment.NewLine + " Please upgrade to licensed version to continue using this app.", false);
                messageWindow.ShowDialog();
                (App.Current as App).Shutdown();
                Environment.Exit(0);
                return;
            }

            if (UzerSetting.FirstRun)
            {
                //Nisam siguran da mi ovo treba
                //if (!IsUserAnAdmin())
                //{
                //    MessageWindow msgWindow = new MessageWindow("It looks like this account is not in the Administrators group. Administrator privileges are required to install this app."
                //        + Environment.NewLine
                //        + "The app is going to exit.");
                //    msgWindow.ShowDialog();
                //    (App.Current as App).Shutdown();
                //    Environment.Exit(0);
                //    return;
                //}

                EULAWindow eulaWindow = new EULAWindow();
                var dialogResult = eulaWindow.ShowDialog();
                if (!dialogResult.HasValue && !dialogResult.Value)
                {
                    (App.Current as App).Shutdown();
                    Environment.Exit(0);
                    return;
                }
            }

            _dataLogger = new DataLogger(_uzerSetting);

            if (deleteOldLogsTask != null)
                deleteOldLogsTask.Wait();

            if (!_uzerSetting.Stealth & !autostart)
            {
                CreateOrShowMainWindow();
                if (_uzerSetting.FirstRun)
                {
                    SetWindowDimensions();
                    _uzerSetting.FirstRun = false;
                }
            }

            MatchSettingsAndRegistry();

            #region Event Handlers

            this.Startup += (s, e) =>
               {
                   SetProcessKillAuth();
               };

            this.SessionEnding += (s, e) =>
            {
                FinishAndExit(true);
            };

            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) =>
            {
                CreateOrShowMainWindow();
            };

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
#if DEBUG
            this.Exit += App_Exit;
#endif
            #endregion

            ShowHideTrayIcon();

            UzerSetting.LastExecutedDate = DateTime.Now;

        }

        void _settingsQueue_SaveSettings(object sender, SettingsProxy e)
        {
            SaveSettings();
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            Exceptions.Logger.DumpDebug(string.Format("App shutting down on {0}, user triggerd = {1}, sender {2}, exit code {3}", DateTime.Now, _userTriggerExit, sender, e.ApplicationExitCode));
        }

        public App()
        {

        }

        #endregion

        #region UserSettings Methods

        private void CreateSettingsAndUsageTypes()
        {
            using (var context = new AppsEntities())
            {
                if (context.Settings.Count() == 0)
                {
                    _settings = new Setting(true);
                    context.Settings.Add(_settings);
                    context.SaveChanges();
                }
                else
                {
                    _settings = context.Settings.FirstOrDefault();
                }

                if (context.UsageTypes.Count() != Enum.GetValues(typeof(UsageTypes)).Length)
                {
                    foreach (var type in Enum.GetNames(typeof(UsageTypes)))
                    {
                        if (context.Usages.Include(u => u.UsageType).Any(u => u.UsageType.UType == type))
                            continue;
                        UsageType usageType = new UsageType() { UType = type };
                        usageType = context.UsageTypes.Add(usageType);

                    }
                    context.Entry(_settings).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }

        public void SaveSettings(string propertyname = "")
        {
            _settingsQueue.Add(_uzerSetting);
            // Task.Factory.StartNew(SaveSettings);
        }

        private async void SaveSettings()
        {
            using (var context = new AppsEntities())
            {
                context.Entry(_settings).State = System.Data.Entity.EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        private void UpdateLogData(string propertyName)
        {
            if (_dataLogger == null)
                return;
            if (propertyName == "RunAtStartup")
                return;

            if (_dataLogger.EnableIdle != UzerSetting.EnableIdle)
                _dataLogger.EnableIdle = UzerSetting.EnableIdle;

            if (_dataLogger.TakeScreenShots != _uzerSetting.TakeScreenshots) _dataLogger.TakeScreenShots = _uzerSetting.TakeScreenshots;

            if (_dataLogger.ScreenShotInterval != _uzerSetting.TimerInterval) _dataLogger.ScreenShotInterval = _uzerSetting.TimerInterval;

            if (_dataLogger.KeyBoardHook.KeyLoggerEnabled != _uzerSetting.EnableKeylogger) _dataLogger.KeyBoardHook.KeyLoggerEnabled = _uzerSetting.EnableKeylogger;

            if (_dataLogger.LoggingStatus.Running() != _uzerSetting.LoggingEnabled)
                _dataLogger.LoggingStatus = _uzerSetting.LoggingEnabled ? LoggingStatus.Running : LoggingStatus.Stopped;

            if (_uzerSetting.EnableEmailReports)
                UpdateEmailSettings(true);
            else
                UpdateEmailSettings(false);

            //Console.WriteLine("Data logger enabled " + dataLogger.EnableFileWatcher);
            //Console.WriteLine("Settings enabled " + uzerSetting.EnableFileWatcher);

            if (_dataLogger.EnableFileWatcher != _uzerSetting.EnableFileWatcher)
            {
                _dataLogger.EnableFileWatcher = _uzerSetting.EnableFileWatcher;
                // Console.WriteLine("Filewatcher is null = " + dataLogger.FileSystemWatcher == null);
                if (_dataLogger.FileSystemWatcher == null)
                    return;
                if (_dataLogger.FileSystemWatcher.Path != _uzerSetting.FileWatcherPath)
                {
                    try
                    {
                        _dataLogger.FileSystemWatcher.Path = _uzerSetting.FileWatcherPath;
                    }
                    catch (Exception ex)
                    {
                        _dataLogger.FileSystemWatcher.Path = _uzerSetting.FileWatcherPath = @"C:\";
                        MessageWindow window = new MessageWindow(ex);
                        window.Show();
                    }
                }

                if (_dataLogger.FileSystemWatcher.IncludeSubdirectories != _uzerSetting.FileWatcherSubdirectories) _dataLogger.FileSystemWatcher.IncludeSubdirectories = _uzerSetting.FileWatcherSubdirectories;
            }
        }

        private void ShowHideTrayIcon()
        {
            if (!_uzerSetting.Stealth)
            {
                if (_trayIcon == null)
                    _trayIcon = new Controls.TrayIcon();
                _trayIcon.IsVisible = true;
            }
            else
            {
                if (_trayIcon != null)
                {
                    _trayIcon.IsVisible = false;
                    _trayIcon.Dispose();
                    _trayIcon = null;
                }
            }
        }

        private void UpdateEmailSettings(bool enable)
        {
            if (enable)
            {
                if (_emailReport == null)
                    _emailReport = new EmailReport();
                _emailReport.EmailTo = _uzerSetting.EmailTo;
                _emailReport.EmailFrom = _uzerSetting.EmailFrom;
                _emailReport.Interval = _uzerSetting.EmailInterval;
                _emailReport.SmtpHost = _uzerSetting.EmailSmtpHost;
                _emailReport.SmtpPort = _uzerSetting.EmailSmtpPort;
                _emailReport.SmtpUsername = _uzerSetting.EmailSmtpUsername;
                _emailReport.SmtpPassword = _uzerSetting.EmailSmtpPassword;
                _emailReport.SSL = _uzerSetting.EmailSSL;
            }
            else
            {
                if (_emailReport != null)
                {
                    _emailReport.StopReporting();
                    _emailReport.Dispose();
                    _emailReport = null;
                }
            }

        }

        #endregion

        private void SetRemoveRunAtStartupRegistry(bool setEntry, bool catchException)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (setEntry)
                {
                    rk.SetValue("app service", System.Reflection.Assembly.GetExecutingAssembly().Location + " -autostart");
                }
                else
                {
                    rk.DeleteValue("app service", false);
                }
            }
            catch (System.Security.SecurityException)
            {
                if (catchException)
                {
                    MessageWindow messageWindow = new MessageWindow(
                        "You don't have administrative privilages to change this option."
                        + Environment.NewLine
                        + "Please try running the app as Administrator." + Environment.NewLine
                        + "Right click on the app or shortcut and select 'Run as Adminstrator'.");
                    messageWindow.ShowDialog();
                    if (_uzerSetting.RunAtStartup) _uzerSetting.RunAtStartup = false;
                    else _uzerSetting.RunAtStartup = true;
                }
            }
        }

        private void MatchSettingsAndRegistry()
        {
            bool? exists = RegistryEntryExists();
            if (exists == null && _uzerSetting.RunAtStartup)
                _uzerSetting.RunAtStartup = false;
            else if (exists.HasValue && exists.Value && !_uzerSetting.RunAtStartup)
                _uzerSetting.RunAtStartup = true;
        }

        private bool? RegistryEntryExists()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk.GetValue("app service") == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool CheckTrialExpiration()
        {
            if (!_uzerSetting.Licence)
            {
                bool expired = false;
                if (_uzerSetting.TrialStartDate.AddDays(15) < DateTime.Now)
                    expired = true;
                if (_uzerSetting.LastExecutedDate.HasValue && _uzerSetting.LastExecutedDate.Value > DateTime.Now)
                    expired = true;
                return expired;
            }
            else
                return false;
        }

        #region Class Methods

        private Task DeleteOldLogsAsync()
        {
            return Task.Run(new Action(DeleteOldLogs));
        }

        private void DeleteOldLogs()
        {
            DateTime dateTreshold = DateTime.Now.Date.AddDays(-1d * UzerSetting.OldLogDeleteDays);
            using (var context = new AppsEntities())
            {
                var logs = (from l in context.Logs
                            where l.DateCreated < dateTreshold
                            select l).Include(l => l.Screenshots).ToList();

                var usages = (from u in context.Usages
                              where u.UsageStart < dateTreshold
                              select u).ToList();

                var fileLogs = (from f in context.FileLogs
                                where f.Date < dateTreshold
                                select f).ToList();

                var blockedApps = (from b in context.BlockedApps
                                   where b.Date < dateTreshold
                                   select b).ToList();

                DeleteBlockedApps(context, blockedApps);

                DeleteFilelogs(context, fileLogs);

                DeleteUsages(context, usages);

                DeleteLogsAndScreenshots(context, logs);

                DeleteEmptyLogs(context);

                context.SaveChanges();
            }
        }

        private void DeleteBlockedApps(AppsEntities context, List<BlockedApp> blockedApps)
        {
            foreach (var blockedApp in blockedApps)
            {
                if (!context.BlockedApps.Local.Any(b => b.BlockedAppID == blockedApp.BlockedAppID))
                    context.BlockedApps.Attach(blockedApp);
                context.BlockedApps.Remove(blockedApp);
            }
        }

        private void DeleteFilelogs(AppsEntities context, List<FileLog> fileLogs)
        {
            foreach (var fileLog in fileLogs)
            {
                if (!context.FileLogs.Local.Any(f => f.FileLogID == fileLog.FileLogID))
                    context.FileLogs.Attach(fileLog);
                context.FileLogs.Remove(fileLog);
            }
        }

        private void DeleteUsages(AppsEntities context, List<Usage> usages)
        {
            foreach (var usage in usages)
            {
                if (!context.Usages.Local.Any(u => u.UsageID == usage.UsageID))
                    context.Usages.Attach(usage);
                context.Usages.Remove(usage);
            }
        }

        private void DeleteLogsAndScreenshots(AppsEntities context, List<Log> logs)
        {
            foreach (var log in logs)
            {

                foreach (var screenshot in log.Screenshots.ToList())
                {
                    if (!context.Screenshots.Local.Any(s => s.ScreenshotID == screenshot.ScreenshotID))
                    {
                        context.Screenshots.Attach(screenshot);
                    }
                    context.Screenshots.Remove(screenshot);
                }

                if (!context.Logs.Local.Any(l => l.LogID == log.LogID))
                {
                    context.Logs.Attach(log);
                }
                context.Entry(log).State = System.Data.Entity.EntityState.Deleted;
            }
        }

        private void DeleteEmptyLogs(AppsEntities context)
        {
            foreach (var window in context.Windows)
            {
                if (window.Logs.Count == 0)
                {
                    if (!context.Windows.Local.Any(w => w.WindowID == window.WindowID))
                        context.Windows.Attach(window);
                    context.Windows.Remove(window);
                }
            }

            foreach (var app in context.Applications)
            {
                if (app.Windows.Count == 0)
                {
                    if (!context.Applications.Local.Any(a => a.ApplicationID == app.ApplicationID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                }
            }
        }

        [Conditional("RELEASE")]
        private void SetProcessKillAuth()
        {
            IntPtr hProcess = WinAPI.GetCurrentProcess();
            var dacl = WinAPI.GetProcessSecurityDescriptor(hProcess);
            dacl.DiscretionaryAcl.InsertAce(0, new CommonAce(AceFlags.None, AceQualifier.AccessDenied,
                (int)WinAPI.ProcessAccessRights.PROCESS_ALL_ACCESS, new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null),
                false, null));
            WinAPI.SetProcessSecurityDescriptor(hProcess, dacl);
        }

        private bool CheckPassword()
        {
            if (!_uzerSetting.IsMasterPasswordSet)
                return true;
            else
            {
                PasswordWindow passwordWindow = new PasswordWindow();
                bool? dialog = passwordWindow.ShowDialog();
                if (dialog.Value)
                {
                    return true;
                }
                return false;
            }
        }

        private bool IsDbCleaningRequired()
        {
            decimal size = Globals.GetDBSize();
            if (size > 3900m)
                return true;
            else
                return false;

        }

        public void CreateOrShowMainWindow()
        {
            if (CheckPassword())
            {
                if (this.MainWindow == null)
                {
                    this.MainWindow = new Task_Logger_Pro.MainWindow();
                    this.MainWindow.Show();
                }
                else
                {
                    if (!this.MainWindow.IsLoaded)
                    {
                        this.MainWindow = new MainWindow();
                        this.MainWindow.Show();
                    }
                    else
                        this.MainWindow.Activate();
                }
            }
        }

        public void CloseMainWindow()
        {
            if (this.MainWindow != null)
            {
                this.MainWindow.Close();
                this.MainWindow = null;
            }
        }


        public void ChangeTheme()
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            if (_uzerSetting.LightTheme)
            {
                if (_uzerSetting.LoggingEnabled)
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/RunningLight.xaml", UriKind.Relative) });
                else
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/StoppedLight.xaml", UriKind.Relative) });
            }
            else
            {
                if (_uzerSetting.LoggingEnabled)
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/Running.xaml", UriKind.Relative) });
                else
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/Themes/Stopped.xaml", UriKind.Relative) });
            }

        }

        private void SetWindowDimensions()
        {
            var bound = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double left, top, width, height;
            left = bound.Left + 50d;
            top = bound.Top + 50d;
            width = bound.Width - 100d;
            height = bound.Height - 100d;
            this.MainWindow.Left = left;
            this.MainWindow.Top = top;
            this.MainWindow.Width = width;
            this.MainWindow.Height = height;
        }

        private bool IsUserAnAdmin()
        {
            bool isAllowed = false;
            using (PrincipalContext pc = new PrincipalContext(ContextType.Machine, null))
            {
                UserPrincipal up = UserPrincipal.Current;
                GroupPrincipal gp = GroupPrincipal.FindByIdentity(pc, "Administrators");
                if (up.IsMemberOf(gp)) isAllowed = true;
            }
            return isAllowed;
        }

        #endregion

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Task_Logger_Pro.Exceptions.Logger.DumpExceptionInfo(e.Exception);
                if (App.UzerSetting != null)
                {
                    if (!App.UzerSetting.Stealth)
                    {
                        MessageWindow messageWindow = new MessageWindow("Ooops, this is awkward ... something went wrong." +
                               Environment.NewLine + "The app needs to close." + Environment.NewLine + "Error: " + e.Exception.Message);
                        messageWindow.ShowDialog();
                    }
                }
            }
            finally
            {
                FinishAndExit(false);
            }
        }

        internal void FinishAndExit(bool userTriggered)
        {
            _userTriggerExit = userTriggered;
            if (App.DataLogger != null)
                App.DataLogger.FinishLogging();
            CloseMainWindow();
            Dispose();
            Application.Current.Shutdown();
            //  Environment.Exit(0);
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
                if (_dataLogger != null) { _dataLogger.Dispose(); _dataLogger = null; }
                if (_emailReport != null) { _emailReport.Dispose(); _emailReport = null; }
                if (_trayIcon != null) { _trayIcon.Dispose(); _trayIcon = null; }
                if (_settingsQueue != null) { _settingsQueue.Dispose(); _settingsQueue = null; }
            }
        }

        #endregion
    }
}

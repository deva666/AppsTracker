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
using Task_Logger_Pro.Cleaner;
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
            //_settingsQueue = new SettingsQueue();
            //_settingsQueue.SaveSettings += _settingsQueue_SaveSettings;

            ChangeTheme();

            _uzerSetting.PropertyChanged += (s, e) =>
            {
                UpdateLogData(e.PropertyName);
                SaveSettings();
                //_settingsQueue.Add(_uzerSetting);          
            };

            if (CheckTrialExpiration())
            {
                LicenceWindow licenceWindow = new LicenceWindow();
                var result = licenceWindow.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    //OK, licence valid
                }
                else
                {
                    (App.Current as App).Shutdown();
                    Environment.Exit(0);
                    return;
                }
            }

            Task deleteOldLogsTask = null;

            if (_uzerSetting.DeleteOldLogs)
                deleteOldLogsTask = Task.Run(() => DBCleaner.DeleteOldScreenshots(UzerSetting.OldLogDeleteDays));

#if PORTABLE_SYMBOL

            if (UzerSetting.FirstRun)
            {
                EULAWindow eulaWindow = new EULAWindow();
                var dialogResult = eulaWindow.ShowDialog();
                if (!dialogResult.HasValue && !dialogResult.Value)
                {
                    (App.Current as App).Shutdown();
                    Environment.Exit(0);
                    return;
                }
            }

#endif

            _dataLogger = new DataLogger(_uzerSetting);

            Globals.DBCleaningRequired += Globals_DBCleaningRequired;
            Globals.GetDBSize();

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

            #endregion

            ShowHideTrayIcon();

            UzerSetting.LastExecutedDate = DateTime.Now;

        }

        void Globals_DBCleaningRequired(object sender, EventArgs e)
        {
            UzerSetting.TakeScreenshots = DataLogger.TakeScreenShots = false;

            if (!UzerSetting.Stealth)
            {
                MessageWindow msgWindow = new MessageWindow("Database size has reached the maximum allowed value" + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);
                msgWindow.ShowDialog();
            }
            Globals.DBCleaningRequired -= Globals_DBCleaningRequired;
        }

        void _settingsQueue_SaveSettings(object sender, SettingsProxy e)
        {
            SaveSettings();
            Console.WriteLine("setting saved");
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

            if (_dataLogger.EnableKeyboardHook != _uzerSetting.EnableKeylogger) _dataLogger.EnableKeyboardHook = _uzerSetting.EnableKeylogger;

            if (_dataLogger.LoggingStatus.Running() != _uzerSetting.LoggingEnabled)
                _dataLogger.LoggingStatus = _uzerSetting.LoggingEnabled ? LoggingStatus.Running : LoggingStatus.Stopped;

            if (_uzerSetting.EnableEmailReports)
                UpdateEmailSettings(true);
            else
                UpdateEmailSettings(false);

            //if (_dataLogger.EnableFileWatcher != _uzerSetting.EnableFileWatcher)
            //{
            //    _dataLogger.EnableFileWatcher = _uzerSetting.EnableFileWatcher;
            //    if (_dataLogger.FileSystemWatcher == null)
            //        return;
            //    if (_dataLogger.FileSystemWatcher.Path != _uzerSetting.FileWatcherPath)
            //    {
            //        try
            //        {
            //            _dataLogger.FileSystemWatcher.Path = _uzerSetting.FileWatcherPath;
            //        }
            //        catch (Exception ex)
            //        {
            //            _dataLogger.FileSystemWatcher.Path = _uzerSetting.FileWatcherPath = @"C:\";
            //            MessageWindow window = new MessageWindow(ex);
            //            window.Show();
            //        }
            //    }

            //    if (_dataLogger.FileSystemWatcher.IncludeSubdirectories != _uzerSetting.FileWatcherSubdirectories) _dataLogger.FileSystemWatcher.IncludeSubdirectories = _uzerSetting.FileWatcherSubdirectories;
            //}
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
            else if (exists.HasValue && !exists.Value && _uzerSetting.RunAtStartup)
                _uzerSetting.RunAtStartup = false;
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
                //if (ConnectionConfig.ExpExists())
                //    return true;
                bool expired = false;
                if (_uzerSetting.TrialStartDate.AddDays(15) < DateTime.Now)
                {
                    expired = true;
                   //ConnectionConfig.FlushExp();
                }
                if (_uzerSetting.LastExecutedDate.HasValue && _uzerSetting.LastExecutedDate.Value > DateTime.Now)
                    expired = true;
                return expired;
            }
            else
                return false;
        }

        #region Class Methods


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

        public void CreateOrShowMainWindow()
        {
            if (CheckPassword())
            {
                if (this.MainWindow == null)
                {
                    this.MainWindow = new Task_Logger_Pro.MainWindow();
                    this.MainWindow.Left = UzerSetting.MainWindowLeft;
                    this.MainWindow.Top = UzerSetting.MainWindowTop;
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Task_Logger_Pro.Controls;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Encryption;
using System.Collections.ObjectModel;
using AppsTracker.Models.EntityModels;
using AppsTracker.DAL;
using AppsTracker.Models.Proxy;

namespace Task_Logger_Pro.ViewModels
{
    public class Settings_generalViewModel : ViewModelBase, IChildVM, ICommunicator
    {
        #region Fields

        bool _popupIntervalIsOpen = false;
        bool _popupUsersToLogIsOpen = false;
        bool _popupBlockedProcessesIsOpen = false;
        bool _popupPasswordIsOpen = false;
        bool _popupEmailReportsIsOpen = false;
        bool _popupFilewatcherFilterIsOpen = false;
        bool _popupEmailIntervalIsOpen = false;
        bool _popupOldLogsIsOpen = false;
        bool _popupidleTimerIsOpen = false;

        string _selectedUserName = Environment.UserName;

        Uzer _selectedUser;
        AppsToBlockProxy _selectedAppToBlock;

        IEnumerable<AppsToBlockProxy> _appsToBlockCollection;
        IEnumerable<Uzer> _allUsersCollection;

        ICommand _changeStealthModeCommand;
        ICommand _changeScreenshotIntervalCommand;
        ICommand _showPopUpCommand;
        ICommand _showFolderBrowserDialogCommand;
        ICommand _removeProcessesFromBlockedCommand;
        ICommand _setPasswordCommand;
        ICommand _setMailConfCommand;
        ICommand _sendTestEmailCommand;
        ICommand _changeEmailIntervalCommand;
        ICommand _showAboutWindowCommand;
        ICommand _changeOldLogsDaysCommand;
        ICommand _changeThemeCommand;
        ICommand _setStartupCommand;
        ICommand _changeIdleTimerCommand;

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return "APP SETTINGS";
            }
        }
        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public SettingsProxy UserSettings
        {
            get
            {
                return App.UzerSetting;
            }
        }

        public Uzer SelectedUser
        {
            get
            {
                return _selectedUser;
            }
            set
            {
                _selectedUser = value;
                LoadAppsToBlock();
                PropertyChanging("");
            }
        }

        public AppsToBlockProxy SelectedAppToBlock
        {
            get
            {
                return _selectedAppToBlock;
            }
            set
            {

                if (_selectedAppToBlock != value)
                {
                    if (_selectedAppToBlock != null)
                        _selectedAppToBlock.PropertyChanged -= _selectedAppToBlock_PropertyChanged;

                    _selectedAppToBlock = value;

                    if (_selectedAppToBlock != null)
                        _selectedAppToBlock.PropertyChanged += _selectedAppToBlock_PropertyChanged;

                    PropertyChanging("SelectedAppToBlock");
                }
            }
        }

        public IEnumerable<AppsToBlockProxy> AppsToBlockCollection
        {
            get
            {
                return _appsToBlockCollection;
            }
            set
            {
                _appsToBlockCollection = value;
                PropertyChanging("AppsToBlockCollection");
            }
        }
        public IEnumerable<Uzer> AllUsersCollection
        {
            get
            {
                if (_allUsersCollection == null)
                    using (var context = new AppsEntities())
                    {
                        _allUsersCollection = context.Users.Include(u => u.AppsToBlocks.Select(ab => ab.Application)).ToList();
                    }
                return _allUsersCollection;
            }
        }

        #region PopupProps

        public bool PopupIntervalIsOpen
        {
            get
            {
                return _popupIntervalIsOpen;
            }
            set
            {
                _popupIntervalIsOpen = value;
                PropertyChanging("PopupIntervalIsOpen");
            }
        }

        public bool PopupUsersToLogIsOpen
        {
            get
            {
                return _popupUsersToLogIsOpen;
            }
            set
            {
                _popupUsersToLogIsOpen = value;
                PropertyChanging("PopupUsersToLogIsOpen");
            }
        }

        public bool PopupBlockedProcessesIsOpen
        {
            get
            {
                return _popupBlockedProcessesIsOpen;
            }
            set
            {
                _popupBlockedProcessesIsOpen = value;
                PropertyChanging("PopupBlockedProcessesIsOpen");
            }
        }

        public bool PopupPasswordIsOpen
        {
            get
            {
                return _popupPasswordIsOpen;
            }
            set
            {
                _popupPasswordIsOpen = value;
                PropertyChanging("PopupPasswordIsOpen");
            }
        }

        public bool PopupEmailReportsIsOpen
        {
            get
            {
                return _popupEmailReportsIsOpen;
            }
            set
            {
                _popupEmailReportsIsOpen = value;
                PropertyChanging("PopupEmailReportsIsOpen");
            }
        }

        public bool PopupFilewatcherFilterIsOpen
        {
            get
            {
                return _popupFilewatcherFilterIsOpen;
            }
            set
            {
                _popupFilewatcherFilterIsOpen = value;
                PropertyChanging("PopupFilewatcherFilterIsOpen");
            }
        }

        public bool PopupEmailIntervalIsOpen
        {
            get
            {
                return _popupEmailIntervalIsOpen;
            }
            set
            {
                _popupEmailIntervalIsOpen = value;
                PropertyChanging("PopupEmailIntervalIsOpen");
            }
        }

        public bool PopupOldLogsIsOpen
        {
            get
            {
                return _popupOldLogsIsOpen;
            }
            set
            {
                _popupOldLogsIsOpen = value;
                PropertyChanging("PopupOldLogsIsOpen");
            }
        }

        public bool PopupIdleTimerIsOpen
        {
            get
            {
                return _popupidleTimerIsOpen;
            }
            set
            {
                _popupidleTimerIsOpen = value;
                PropertyChanging("PopupIdleTimerIsOpen");
            }
        }
        #endregion

        #region EmailProps

        public bool EnableEmailReports
        {
            get
            {
                return App.UzerSetting.EnableEmailReports;
            }
            set
            {
                App.UzerSetting.EnableEmailReports = value;
                PropertyChanging("EnableEmailReports");
            }
        }

        public bool EmailSSL
        {
            get
            {
                return App.UzerSetting.EmailSSL;
            }
            set
            {
                App.UzerSetting.EmailSSL = value;
                PropertyChanging("EmailSSL");
            }
        }

        public string EmailTo
        {
            get
            {
                return App.UzerSetting.EmailTo;
            }
            set
            {
                App.UzerSetting.EmailTo = value;
                PropertyChanging("EmailTo");
            }
        }

        public string EmailFrom
        {
            get
            {
                return App.UzerSetting.EmailFrom;
            }
            set
            {
                App.UzerSetting.EmailFrom = value;
                PropertyChanging("EmailFrom");
            }
        }

        public string EmailSmtpHost
        {
            get
            {
                return App.UzerSetting.EmailSmtpHost;
            }
            set
            {
                App.UzerSetting.EmailSmtpHost = value;
                PropertyChanging("EmailSmtpHost");
            }
        }

        public string EmailSmtpUsername
        {
            get
            {
                return App.UzerSetting.EmailSmtpUsername;
            }
            set
            {
                App.UzerSetting.EmailSmtpUsername = value;
                PropertyChanging("EmailSmtpUsername");
            }
        }

        public string EmailSmtpPassword
        {
            get
            {
                return App.UzerSetting.EmailSmtpPassword;
            }
            set
            {
                App.UzerSetting.EmailSmtpPassword = value;
                PropertyChanging("EmailSmtpPassword");
            }
        }

        public int EmailSmtpPort
        {
            get
            {
                return App.UzerSetting.EmailSmtpPort;
            }
            set
            {
                App.UzerSetting.EmailSmtpPort = value;
                PropertyChanging("EmailSmtpPort");
            }
        }

        public double EmailInterval
        {
            get
            {
                return App.UzerSetting.EmailInterval;
            }
        }

        #endregion

        public bool IsMasterPasswordSet
        {
            get
            {
                return App.UzerSetting.IsMasterPasswordSet;
            }
            set
            {
                App.UzerSetting.IsMasterPasswordSet = value;
                PropertyChanging("IsMasterPasswordSet");
            }
        }

        public string SelectedUserName
        {
            get
            {
                return _selectedUserName;
            }
            set
            {
                _selectedUserName = value;
                PropertyChanging("SelectedUserName");
            }
        }


        #region CommandProps

        public ICommand ChangeStealthModeCommand
        {
            get
            {
                return _changeStealthModeCommand == null ? _changeStealthModeCommand = new DelegateCommand(ChangeStealthMode) : _changeStealthModeCommand;
            }
        }

        public ICommand ChangeScreenShotIntervalCommand
        {
            get
            {
                return _changeScreenshotIntervalCommand == null ? _changeScreenshotIntervalCommand = new DelegateCommand(ChangeScreenshotInterval) : _changeScreenshotIntervalCommand;
            }
        }

        public ICommand ShowPopupCommand
        {
            get
            {
                return _showPopUpCommand == null ? _showPopUpCommand = new DelegateCommand(ShowPopUp) : _showPopUpCommand;
            }
        }

        public ICommand ShowFolderBrowserDialogCommand
        {
            get
            {
                return _showFolderBrowserDialogCommand == null ? _showFolderBrowserDialogCommand = new DelegateCommand(ShowFolderBrowserDialog) : _showFolderBrowserDialogCommand;
            }
        }

        public ICommand RemoveProcessFromBlockedCommand
        {
            get
            {
                return _removeProcessesFromBlockedCommand == null ? _removeProcessesFromBlockedCommand = new DelegateCommand(RemoveFromBlockedCollection) : _removeProcessesFromBlockedCommand;
            }
        }

        public ICommand SetPasswordCommand
        {
            get
            {
                return _setPasswordCommand == null ? _setPasswordCommand = new DelegateCommand(SetPassword) : _setPasswordCommand;
            }
        }

        public ICommand SetMailConfCommand
        {
            get
            {
                return _setMailConfCommand == null ? _setMailConfCommand = new DelegateCommand(SetMailConf) : _setMailConfCommand;
            }
        }

        public ICommand SendTestEmailCommand
        {
            get
            {
                return _sendTestEmailCommand == null ? _sendTestEmailCommand = new DelegateCommand(SendTestEmailAsync) : _sendTestEmailCommand;
            }
        }

        public ICommand ChangeEmailIntervalCommand
        {
            get
            {
                return _changeEmailIntervalCommand == null ? _changeEmailIntervalCommand = new DelegateCommand(ChangeEmailInterval) : _changeEmailIntervalCommand;
            }
        }

        public ICommand ShowAboutWindowCommand
        {
            get
            {
                return _showAboutWindowCommand == null ? _showAboutWindowCommand = new DelegateCommand(ShowAboutWindow) : _showAboutWindowCommand;
            }
        }

        public ICommand ChangeOldLogsDaysCommand
        {
            get
            {
                return _changeOldLogsDaysCommand == null ? _changeOldLogsDaysCommand = new DelegateCommand(ChangeOldLogsDays) : _changeOldLogsDaysCommand;
            }
        }

        public ICommand ChangeThemeCommand
        {
            get
            {
                return _changeThemeCommand == null ? _changeThemeCommand = new DelegateCommand(ChangeTheme) : _changeThemeCommand;
            }
        }

        public ICommand SetStartupCommand
        {
            get
            {
                return _setStartupCommand == null ? _setStartupCommand = new DelegateCommand(SetStartup) : _setStartupCommand;
            }
        }
        public ICommand ChangeIdleTimerCommand
        {
            get
            {
                return _changeIdleTimerCommand == null ? _changeIdleTimerCommand = new DelegateCommand(ChangeIdleTimer) : _changeIdleTimerCommand;
            }
        }

        #endregion

        #endregion

        #region Constructor

        public Settings_generalViewModel()
        {

        }

        #endregion

        public void LoadContent() { }

        #region Class Methods

        private void ShowMessageWindow(string text)
        {
            MessageWindow messageWindow = new MessageWindow(text);
            messageWindow.ShowDialog();
        }

        private void ShowFolderBrowserDialog(object parameter)
        {
            string toUpdate = parameter as string;
            string path;

            if (toUpdate == null)
                return;

            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                path = dialog.SelectedPath;
            else
                return;

            if (toUpdate.ToUpper() == "FILEWATCHER")
                UserSettings.FileWatcherPath = path;
            else if (toUpdate.ToUpper() == "SCREENSHOTS")
                UserSettings.DefaultScreenshotSavePath = path;

        }

        private void SetGmailConf()
        {
            EmailSmtpHost = "smtp.gmail.com";
            EmailSmtpPort = 587;
            EmailSSL = true;
        }

        private void SetYahooConf()
        {
            EmailSmtpHost = "smtp.mail.yahoo.com";
            EmailSmtpPort = 587;
            EmailSSL = false;
        }

        private void LoadAppsToBlock()
        {
            if (_selectedUser == null)
                return;
            List<AppsToBlockProxy> proxyCollection;
            using (var context = new AppsEntities())
            {
                var appsToBlock = context.AppsToBlocks.Where(a => a.UserID == _selectedUser.UserID).ToList();
                proxyCollection = new List<AppsToBlockProxy>(appsToBlock.Count);
                foreach (var item in appsToBlock)
                {
                    AppsToBlockProxy proxy = item;
                    proxyCollection.Add(proxy);
                }
            }
            AppsToBlockCollection = proxyCollection;
        }

        #endregion

        #region Command Methods

        private void SetPassword(object parameter)
        {
            using (var context = new AppsEntities())
            {
                var passwords = parameter as PasswordBox[];

                if (passwords != null)
                {
                    if (IsMasterPasswordSet)
                    {
                        string currentPassword = Encrypt.GetHashSHA2String(passwords[2].Password);
                        string storedPassword = App.UzerSetting.WindowOpen;
                        if (storedPassword == null) { IsMasterPasswordSet = false; return; }
                        if (currentPassword != storedPassword)
                        {
                            MessageWindow messageWindow = new MessageWindow("Wrong current password.");
                            messageWindow.Owner = Application.Current.MainWindow;
                            messageWindow.ShowDialog();
                            return;
                        }
                    }
                    string password = passwords[0].Password;
                    string confirmPassword = passwords[1].Password;
                    if (password != confirmPassword)
                    {
                        MessageWindow messageWindow = new MessageWindow("Passwords don't match.");
                        messageWindow.Owner = Application.Current.MainWindow;
                        messageWindow.ShowDialog();
                        return;
                    }
                    if (!string.IsNullOrEmpty(password.Trim()))
                    {
                        IsMasterPasswordSet = true;
                        App.UzerSetting.WindowOpen = Encrypt.GetHashSHA2String(password);
                        MessageWindow messageWindow = new MessageWindow("Password set.");
                        messageWindow.ShowDialog();
                    }
                    else
                    {
                        IsMasterPasswordSet = false;
                        App.UzerSetting.WindowOpen = "";
                    }
                }
            }

        }

        private void RemoveFromBlockedCollection(object parameter)
        {
            if (parameter != null)
            {
                ObservableCollection<object>[] paramArray = parameter as ObservableCollection<object>[];

                List<Uzer> usersList = paramArray[0].Cast<Uzer>().ToList();
                if (usersList != null)
                {
                    string userName = usersList.ElementAtOrDefault(0).Name;
                    List<AppsToBlockProxy> appsList = paramArray[1].Cast<AppsToBlockProxy>().ToList();
                    if (userName != null && appsList != null)
                    {
                        using (var context = new AppsEntities())
                        {
                            foreach (var app in appsList)
                            {
                                var appDB = context.AppsToBlocks.SingleOrDefault(a => a.AppsToBlockID == app.AppsToBlockID);
                                if (appDB != null)
                                    context.AppsToBlocks.Remove(appDB);
                            }
                            context.SaveChanges();
                            _allUsersCollection = null;
                            PropertyChanging("AllUsersCollection");
                        }
                    }
                }
            }
        }

        private void ChangeStealthMode()
        {
            if (UserSettings.Stealth)
                UserSettings.Stealth = false;
            else
                UserSettings.Stealth = true;
        }

        private void ChangeScreenshotInterval(object sourceLabel)
        {
            Label label = sourceLabel as Label;
            if (label != null)
            {
                switch (label.Content as string)
                {
                    case "10 sec":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.TenSeconds;
                        break;
                    case "30 sec":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.ThirtySeconds;
                        break;
                    case "1 min":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.OneMinute;
                        break;
                    case "2 min":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.TwoMinute;
                        break;
                    case "5 min":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.FiveMinute;
                        break;
                    case "10 min":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.TenMinute;
                        break;
                    case "30 min":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.ThirtyMinute;
                        break;
                    case "1 hr":
                        UserSettings.ScreenshotInterval = ScreenShotInterval.OneHour;
                        break;
                    default:
                        break;
                }
                PopupIntervalIsOpen = false;
            }
        }

        private void ChangeEmailInterval(object sourceLabel)
        {
            Label label = sourceLabel as Label;
            if (label != null)
            {
                switch (label.Content as string)
                {
                    case "5 min":
                        UserSettings.EmailIntervalEnum = AppsTracker.Models.EntityModels.EmailInterval.FiveMinute;
                        break;
                    case "10 min":
                        UserSettings.EmailIntervalEnum = AppsTracker.Models.EntityModels.EmailInterval.TenMinute;
                        break;
                    case "30 min":
                        UserSettings.EmailIntervalEnum = AppsTracker.Models.EntityModels.EmailInterval.ThirtyMinute;
                        break;
                    case "1 hr":
                        UserSettings.EmailIntervalEnum = AppsTracker.Models.EntityModels.EmailInterval.OneHour;
                        break;
                    case "2 hr":
                        UserSettings.EmailIntervalEnum = AppsTracker.Models.EntityModels.EmailInterval.TwoHour;
                        break;
                    default:
                        break;
                }
                PopupEmailIntervalIsOpen = false;
            }
        }

        private void ShowPopUp(object popupSource)
        {
            string popup = (string)popupSource;
            if (popup == "Users")
            {
                if (PopupUsersToLogIsOpen)
                    PopupUsersToLogIsOpen = false;
                else
                    PopupUsersToLogIsOpen = true;
            }
            else if (popup == "BlockedApplications")
            {
                if (PopupBlockedProcessesIsOpen)
                    PopupBlockedProcessesIsOpen = false;
                else
                    PopupBlockedProcessesIsOpen = true;
            }
            else if (popup == "Password")
            {
                if (PopupPasswordIsOpen)
                    PopupPasswordIsOpen = false;
                else
                    PopupPasswordIsOpen = true;
            }
            else if (popup == "EmailReports")
            {
                if (PopupEmailReportsIsOpen)
                    PopupEmailReportsIsOpen = false;
                else
                    PopupEmailReportsIsOpen = true;
            }
            else if (popup == "FilewatcherFilter")
            {
                if (PopupFilewatcherFilterIsOpen)
                    PopupFilewatcherFilterIsOpen = false;
                else
                    PopupFilewatcherFilterIsOpen = true;
            }
            else if (popup == "EmailInterval")
            {
                if (PopupEmailIntervalIsOpen)
                    PopupEmailIntervalIsOpen = false;
                else
                    PopupEmailIntervalIsOpen = true;
            }
            else if (popup == "OldLogs")
            {
                if (PopupOldLogsIsOpen)
                    PopupOldLogsIsOpen = false;
                else
                    PopupOldLogsIsOpen = true;
            }
            else if (popup == "IdleTimer")
            {
                if (PopupIdleTimerIsOpen)
                    PopupIdleTimerIsOpen = false;
                else
                    PopupIdleTimerIsOpen = true;
            }
            else
                PopupIntervalIsOpen = true;
        }

        private void SetMailConf(object parameter)
        {
            if ((parameter as string) == "Gmail")
                SetGmailConf();
            else
                SetYahooConf();
        }

        private void SendTestEmailAsync()
        {
            Task task = Task.Factory.StartNew(SendTestEmail);
            try
            {
                using (var wait = new WaitCursor())
                {
                    task.Wait();
                }
                ShowMessageWindow("Email sent successfully.");
            }
            catch (Exception ex)
            {
                MessageWindow messageWindow = new MessageWindow(ex);
                messageWindow.ShowDialog();
                //ShowMessageWindow("Email test failed, please check your settings.");
            }
        }

        private void SendTestEmail()
        {
            MailMessage mailMessage = new MailMessage(new MailAddress(UserSettings.EmailFrom), new MailAddress(UserSettings.EmailTo));
            mailMessage.Subject = "Apps Tracker Test Email";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "<h3>It worked!</h3><br/><h4>You are set for receiving email reports</h4>";
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = UserSettings.EmailSmtpHost;
            smtpClient.Credentials = new NetworkCredential(UserSettings.EmailSmtpUsername, UserSettings.EmailSmtpPassword);
            smtpClient.EnableSsl = UserSettings.EmailSSL;
            smtpClient.Port = UserSettings.EmailSmtpPort;
            smtpClient.Send(mailMessage);
        }


        private void ShowAboutWindow()
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }


        private void ChangeOldLogsDays(object parameter)
        {
            string daysString = parameter as string;
            if (daysString != null)
            {
                short days;
                short.TryParse(daysString, out days);
                UserSettings.OldLogDeleteDays = days;
            }
            PopupOldLogsIsOpen = false;
        }

        private void ChangeTheme(object parameter)
        {
            if ((parameter as string) == "Light")
                UserSettings.LightTheme = true;
            else if ((parameter as string) == "Dark")
                UserSettings.LightTheme = false;
            (App.Current as App).ChangeTheme();
        }


        private void ChangeIdleTimer(object parameter)
        {
            string interval = (string)parameter;
            int intOut;
            if (int.TryParse(interval, out intOut))
            {
                UserSettings.IdleInterval = intOut * 60 * 1000;
            }
            PopupIdleTimerIsOpen = false;
        }

        private void SetStartup()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (!UserSettings.RunAtStartup)
                {
                    rk.SetValue("app service", System.Reflection.Assembly.GetExecutingAssembly().Location + " -autostart");
                    UserSettings.RunAtStartup = true;
                }
                else
                {
                    rk.DeleteValue("app service", false);
                    UserSettings.RunAtStartup = false;
                }
            }
            catch (System.Security.SecurityException)
            {

                MessageWindow messageWindow = new MessageWindow("You don't have administrative privilages to change this option." + Environment.NewLine + "Please try running the app as Administrator." + Environment.NewLine
                               + "Right click on the app or shortcut and select 'Run as Adminstrator'.");
                messageWindow.ShowDialog();
            }

        }

        #endregion

        void _selectedAppToBlock_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            AppsToBlockProxy proxy = _selectedAppToBlock;
            if (proxy == null)
                return;
            using (var context = new AppsEntities())
            {
                var appToBlock = context.AppsToBlocks.SingleOrDefault(a => a.AppsToBlockID == proxy.AppsToBlockID);
                if (appToBlock == null)
                    return;
                appToBlock.MapProxyValues(proxy);
                context.Entry(appToBlock).State = EntityState.Modified;
                context.SaveChanges();
                if (proxy.UserID == Globals.UserID)
                {
                    var refreshedAppsToBlock = context.AppsToBlocks.Where(a => a.UserID == Globals.UserID).Include(a=>a.Application).ToList();
                    Mediator.NotifyColleagues<List<AppsToBlock>>(MediatorMessages.AppsToBlockChanged, refreshedAppsToBlock);
                }
            }
        }


        public Mediator Mediator
        {
            get { return Mediator.Instance; }
        }
    }
}

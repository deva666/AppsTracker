#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using AppsTracker.Logging.Helpers;
using AppsTracker.Service;
using AppsTracker.Widgets;
using Microsoft.Win32;

namespace AppsTracker.Controllers
{
    [Export(typeof(IApplicationController))]
    internal sealed class ApplicationController : IApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ILoggingController loggingController;
        private readonly ISyncContext syncContext;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly ILoggingService loggingService;
        private readonly IMessageService messageService;
        private readonly ITrayIcon trayIcon;
        private readonly ExportFactory<IWindow> mainWindowValueFactory;
        private readonly ExportFactory<IPasswordWindow> passwordWindowValueFactory;

        private IWindow mainWindow;        

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController, ILoggingController loggingController,
                                     ISyncContext syncContext, ISqlSettingsService sqlSettingsService,
                                     IXmlSettingsService xmlSettingsService, ILoggingService loggingService,
                                     ITrayIcon trayIcon, IMessageService messageService, ExportFactory<IWindow> windowValueFactory,
                                     ExportFactory<IPasswordWindow> passwordWindowValueFactory)
        {
            this.appearanceController = appearanceController;
            this.loggingController = loggingController;
            this.syncContext = syncContext;
            this.xmlSettingsService = xmlSettingsService;
            this.sqlSettingsService = sqlSettingsService;
            this.loggingService = loggingService;
            this.messageService = messageService;
            this.mainWindowValueFactory = windowValueFactory;
            this.passwordWindowValueFactory = passwordWindowValueFactory;
            this.trayIcon = trayIcon;
        }

        public void Initialize(bool autoStart)
        {
            syncContext.Context = System.Threading.SynchronizationContext.Current;
            xmlSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(sqlSettingsService, OnSettingsChanged, "Settings");

            appearanceController.Initialize(sqlSettingsService.Settings);
            loggingController.Initialize(sqlSettingsService.Settings);

            ReadSettingsFromRegistry();

            if (autoStart == false)
                CreateOrShowMainWindow();

            FirstRunWindowSetup();

            InitializeTrayIcon();

            loggingService.DbSizeCritical += OnDbSizeCritical;
            loggingService.GetDBSize();

            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => CreateOrShowMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            loggingController.SettingsChanging(sqlSettingsService.Settings);
            appearanceController.SettingsChanging(sqlSettingsService.Settings);
        }


        private void ReadSettingsFromRegistry()
        {
            var settings = sqlSettingsService.Settings;

            bool? exists = RegistryEntryExists();
            if (exists == null && settings.RunAtStartup)
                settings.RunAtStartup = false;
            else if (exists.HasValue && exists.Value && !settings.RunAtStartup)
                settings.RunAtStartup = true;
            else if (exists.HasValue && !exists.Value && settings.RunAtStartup)
                settings.RunAtStartup = false;

            sqlSettingsService.SaveChanges(settings);
        }

        private bool? RegistryEntryExists()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key.GetValue("app service") == null)
                    return false;
                return true;
            }
            catch
            {
                return null;
            }
        }

        private void FirstRunWindowSetup()
        {
            if (sqlSettingsService.Settings.FirstRun)
            {
                SetInitialWindowDimensions();
                var settings = sqlSettingsService.Settings;
                settings.FirstRun = false;
                sqlSettingsService.SaveChanges(settings);
            }
        }

        private void InitializeTrayIcon()
        {
            trayIcon.ShowApp.Click += (s, e) => CreateOrShowMainWindow();
            trayIcon.IsVisible = true;
        }

        private void CreateOrShowMainWindow()
        {
            if (CanOpenMainWindow())
            {
                if (mainWindow == null)
                {
                    mainWindow = mainWindowValueFactory.CreateExport().Value;
                    ShowMainWindow();
                }
                else
                {
                    if (!mainWindow.IsLoaded)
                    {
                        ShowMainWindow();
                    }
                    else
                    {
                        mainWindow.Activate();
                    }
                }
            }
        }

        private void ShowMainWindow()
        {
            mainWindow.Closing += (s, e) => SaveWindowPosition();
            LoadWindowPosition();
            mainWindow.Show();
        }

        private bool CanOpenMainWindow()
        {
            if (sqlSettingsService.Settings.IsMasterPasswordSet)
            {
                var passwordWindow = passwordWindowValueFactory.CreateExport().Value;
                bool? dialog = passwordWindow.ShowDialog();
                if (dialog.Value)
                {
                    return true;
                }
                return false;
            }
            else
                return true;
        }

        private void LoadWindowPosition()
        {
            var mainWindowSettings = xmlSettingsService.MainWindowSettings;
            mainWindow.Left = mainWindowSettings.Left;
            mainWindow.Top = mainWindowSettings.Top;
            mainWindow.Width = mainWindowSettings.Width;
            mainWindow.Height = mainWindowSettings.Height;
        }

        private void CloseMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }

        private void SaveWindowPosition()
        {
            xmlSettingsService.MainWindowSettings.Height = mainWindow.Height;
            xmlSettingsService.MainWindowSettings.Width = mainWindow.Width;
            xmlSettingsService.MainWindowSettings.Left = mainWindow.Left;
            xmlSettingsService.MainWindowSettings.Top = mainWindow.Top;
            mainWindow = null;
        }

        private void SetInitialWindowDimensions()
        {
            var bound = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double left, top, width, height;
            left = bound.Left + 50d;
            top = bound.Top + 50d;
            width = bound.Width - 100d;
            height = bound.Height - 100d;
            mainWindow.Left = left;
            mainWindow.Top = top;
            mainWindow.Width = width;
            mainWindow.Height = height;
        }

        private async void OnDbSizeCritical(object sender, EventArgs e)
        {
            var settings = sqlSettingsService.Settings;
            settings.TakeScreenshots = false;
            await sqlSettingsService.SaveChangesAsync(settings);

            messageService.ShowDialog("Database size has reached the maximum allowed value"
                + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);

            loggingService.DbSizeCritical -= OnDbSizeCritical;
        }

        public void ShutDown()
        {
            CloseMainWindow();
            xmlSettingsService.ShutDown();
            loggingController.Dispose();
            trayIcon.Dispose();
        }
    }
}

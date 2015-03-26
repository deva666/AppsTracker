#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using AppsTracker.Data.Service;
using AppsTracker.Views;
using Microsoft.Win32;

namespace AppsTracker.Controllers
{
    [Export(typeof(IApplicationController))]
    internal sealed class ApplicationController : IApplicationController
    {
        private readonly IAppearanceController appearanceController;
        private readonly ILoggingController loggingController;

        private ISqlSettingsService settingsService;
        private IXmlSettingsService xmlSettingsService;

        private TrayIcon trayIcon;
        private Window mainWindow;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController, ILoggingController loggingController)
        {
            this.appearanceController = appearanceController;
            this.loggingController = loggingController;
        }

        public void Initialize(bool autoStart)
        {
            settingsService = ServiceFactory.Get<ISqlSettingsService>();
            xmlSettingsService = ServiceFactory.Get<IXmlSettingsService>();
            xmlSettingsService.Initialize();
            PropertyChangedEventManager.AddHandler(settingsService, OnSettingsChanged, "Settings");

            appearanceController.Initialize(settingsService.Settings);
            loggingController.Initialize(settingsService.Settings);

#if PORTABLE_SYMBOL

ShowEULAWindow();

#endif
            ReadSettingsFromRegistry();

            if (autoStart == false)
                CreateOrShowMainWindow();

            FirstRunWindowSetup();

            ShowTrayIcon();

            Globals.DBCleaningRequired += Globals_DBCleaningRequired;
            Globals.GetDBSize();

            EntryPoint.SingleInstanceManager.SecondInstanceActivating += (s, e) => CreateOrShowMainWindow();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            loggingController.SettingsChanging(settingsService.Settings);
            appearanceController.SettingsChanging(settingsService.Settings);
        }

        private void ShowEULAWindow()
        {
            if (settingsService.Settings.FirstRun)
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
        }

        private void ReadSettingsFromRegistry()
        {
            bool? exists = RegistryEntryExists();
            if (exists == null && settingsService.Settings.RunAtStartup)
                settingsService.Settings.RunAtStartup = false;
            else if (exists.HasValue && exists.Value && !settingsService.Settings.RunAtStartup)
                settingsService.Settings.RunAtStartup = true;
            else if (exists.HasValue && !exists.Value && settingsService.Settings.RunAtStartup)
                settingsService.Settings.RunAtStartup = false;
        }

        private bool? RegistryEntryExists()
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk.GetValue("app service") == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void FirstRunWindowSetup()
        {
            if (settingsService.Settings.FirstRun)
            {
                SetInitialWindowDimensions();
                var settings = settingsService.Settings;
                settings.FirstRun = false;
                settingsService.SaveChanges(settings);
            }
        }

        private void ShowTrayIcon()
        {
            if (trayIcon == null)
                trayIcon = new Views.TrayIcon();
            trayIcon.ShowApp.Click += (s, e) => CreateOrShowMainWindow();
            trayIcon.IsVisible = true;
        }

        private void CreateOrShowMainWindow()
        {
            if (CheckPassword())
            {
                if (mainWindow == null)
                {
                    CreateMainWindow();
                    LoadWindowPosition();
                    mainWindow.Show();
                }
                else
                {
                    if (!mainWindow.IsLoaded)
                    {
                        CreateMainWindow();
                        LoadWindowPosition();
                        mainWindow.Show();
                    }
                    else
                        mainWindow.Activate();
                }
            }
        }

        private void CreateMainWindow()
        {
            mainWindow = new AppsTracker.MainWindow();
            mainWindow.Closing += (s, e) => SaveWindowPosition();
        }

        private bool CheckPassword()
        {
            if (settingsService.Settings.IsMasterPasswordSet)
            {
                PasswordWindow passwordWindow = new PasswordWindow();
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
                SaveWindowPosition();
                mainWindow.Close();
                mainWindow = null;
            }
        }

        private void SaveWindowPosition()
        {
            xmlSettingsService.MainWindowSettings.Height = mainWindow.Height;
            xmlSettingsService.MainWindowSettings.Width = mainWindow.Width;
            xmlSettingsService.MainWindowSettings.Left = mainWindow.Left;
            xmlSettingsService.MainWindowSettings.Top = mainWindow.Top;
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

        private async void Globals_DBCleaningRequired(object sender, EventArgs e)
        {
            var settings = settingsService.Settings;
            settings.TakeScreenshots = false;
            await settingsService.SaveChangesAsync(settings);

            MessageWindow msgWindow = new MessageWindow("Database size has reached the maximum allowed value" + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);
            msgWindow.ShowDialog();

            Globals.DBCleaningRequired -= Globals_DBCleaningRequired;
        }

        public void ShutDown()
        {
            CloseMainWindow();
            xmlSettingsService.ShutDown();
            loggingController.Dispose();
            if (trayIcon != null)
            {
                trayIcon.Dispose();
                trayIcon = null;
            }
        }

    }
}

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
using AppsTracker.Controls;
using AppsTracker.DAL.Service;
using Microsoft.Win32;

namespace AppsTracker.Controllers
{
    [Export(typeof(IApplicationController))]
    internal sealed class ApplicationController : IApplicationController
    {
        private IAppearanceController _appearanceController;
        private ILoggingController _loggingController;

        private ISettingsService _settingsService;

        private TrayIcon _trayIcon;
        private Window _mainWindow;

        [ImportingConstructor]
        public ApplicationController(IAppearanceController appearanceController, ILoggingController loggingController)
        {
            _appearanceController = appearanceController;
            _loggingController = loggingController;
        }

        public void Initialize(bool autoStart)
        {
            _settingsService = ServiceFactory.Get<ISettingsService>();
            PropertyChangedEventManager.AddHandler(_settingsService, OnSettingsChanged, "Settings");

            _appearanceController.Initialize(_settingsService.Settings);
            _loggingController.Initialize(_settingsService.Settings);

#if PORTABLE_SYMBOL

ShowEULAWindow();

#endif
            MatchSettingsAndRegistry();

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
            _loggingController.SettingsChanging(_settingsService.Settings);
            _appearanceController.SettingsChanging(_settingsService.Settings);
        }

        private void ShowEULAWindow()
        {

            if (_settingsService.Settings.FirstRun)
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

        private void MatchSettingsAndRegistry()
        {
            bool? exists = RegistryEntryExists();
            if (exists == null && _settingsService.Settings.RunAtStartup)
                _settingsService.Settings.RunAtStartup = false;
            else if (exists.HasValue && exists.Value && !_settingsService.Settings.RunAtStartup)
                _settingsService.Settings.RunAtStartup = true;
            else if (exists.HasValue && !exists.Value && _settingsService.Settings.RunAtStartup)
                _settingsService.Settings.RunAtStartup = false;
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

        private void FirstRunWindowSetup()
        {
            if (_settingsService.Settings.FirstRun)
            {
                SetInitialWindowDimensions();
                var settings = _settingsService.Settings;
                settings.FirstRun = false;
                _settingsService.SaveChanges(settings);
            }
        }

        private void ShowTrayIcon()
        {
            if (_trayIcon == null)
                _trayIcon = new Controls.TrayIcon();
            _trayIcon.ShowApp.Click += (s, e) => CreateOrShowMainWindow();
            _trayIcon.IsVisible = true;
        }

        private void CreateOrShowMainWindow()
        {
            if (CheckPassword())
            {
                if (_mainWindow == null)
                {
                    _mainWindow = new AppsTracker.MainWindow();
                    LoadWindowPosition();
                    _mainWindow.Show();
                }
                else
                {
                    if (!_mainWindow.IsLoaded)
                    {
                        _mainWindow = new MainWindow();
                        _mainWindow.Show();
                    }
                    else
                        _mainWindow.Activate();
                }
            }
        }

        private bool CheckPassword()
        {
            if (_settingsService.Settings.IsMasterPasswordSet)
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
            var settings = _settingsService.Settings;
            _mainWindow.Left = settings.MainWindowLeft;
            _mainWindow.Top = settings.MainWindowTop;
            _mainWindow.Width = settings.MainWindowWidth;
            _mainWindow.Height = settings.MainWindowHeight;
        }

        private void CloseMainWindow()
        {
            if (_mainWindow != null)
            {
                SaveWindowPosition();
                _mainWindow.Close();
                _mainWindow = null;
            }
        }

        private void SaveWindowPosition()
        {
            var settings = _settingsService.Settings;
            settings.MainWindowHeight = _mainWindow.Height;
            settings.MainWindowWidth = _mainWindow.Width;
            settings.MainWindowLeft = _mainWindow.Left;
            settings.MainWindowTop = _mainWindow.Top;
            _settingsService.SaveChanges(settings);
        }

        private void SetInitialWindowDimensions()
        {
            var bound = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double left, top, width, height;
            left = bound.Left + 50d;
            top = bound.Top + 50d;
            width = bound.Width - 100d;
            height = bound.Height - 100d;
            _mainWindow.Left = left;
            _mainWindow.Top = top;
            _mainWindow.Width = width;
            _mainWindow.Height = height;
        }

        private async void Globals_DBCleaningRequired(object sender, EventArgs e)
        {
            var settings = _settingsService.Settings;
            settings.TakeScreenshots = false;
            await _settingsService.SaveChangesAsync(settings);

            if (!settings.Stealth)
            {
                MessageWindow msgWindow = new MessageWindow("Database size has reached the maximum allowed value" + Environment.NewLine + "Please run the screenshot cleaner from the settings menu to continue capturing screenshots.", false);
                msgWindow.ShowDialog();
            }
            Globals.DBCleaningRequired -= Globals_DBCleaningRequired;
        }

        public void ShutDown()
        {
            _loggingController.Dispose();
            CloseMainWindow();
            if (_trayIcon != null) { _trayIcon.Dispose(); _trayIcon = null; }
        }

    }
}

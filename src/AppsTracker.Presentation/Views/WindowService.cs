using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Views;
using AppsTracker.Widgets;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Settings;

namespace AppsTracker.Service
{
    [Export(typeof(IWindowService))]
    public sealed class WindowService : IWindowService
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IUserSettingsService userSettingsService;
        private readonly ITrayIcon trayIcon;
        private readonly IEnumerable<ExportFactory<IShell, IShellMetaData>> shellFactories;

        private readonly MeasureProvider measureProvider;

        private IShell mainWindow;
        private IShell limitNotifyWindow;

        [ImportingConstructor]
        public WindowService(IAppSettingsService appSettingsService,
                             IUserSettingsService userSettingsService,
                             ITrayIcon trayIcon,
                             [ImportMany]IEnumerable<ExportFactory<IShell, IShellMetaData>> shellFactories,
                             MeasureProvider measureProvider)
        {
            this.appSettingsService = appSettingsService;
            this.userSettingsService = userSettingsService;
            this.trayIcon = trayIcon;
            this.shellFactories = shellFactories;
            this.measureProvider = measureProvider;

            limitNotifyWindow = GetShell("Limit toast window");
            limitNotifyWindow.Show();
        }

        public void ShowMessageDialog(string message, bool showCancel = true)
        {
            var msgWindow = GetMessageWindow();
            msgWindow.ViewArgument = message;
            msgWindow.ShowDialog();
        }

        public void ShowMessageDialog(Exception fail)
        {
            var msgWindow = GetMessageWindow();
            msgWindow.ViewArgument = fail;
            msgWindow.ShowDialog();
        }

        public void ShowMessage(string message, bool showCancel = true)
        {
            var msgWindow = GetMessageWindow();
            msgWindow.ViewArgument = message;
            msgWindow.Show();
        }

        public void ShowMessage(Exception fail)
        {
            var msgWindow = GetMessageWindow();
            msgWindow.ViewArgument = fail;
            msgWindow.Show();
        }

        private IShell GetMessageWindow()
        {
            var factory = shellFactories.Single(s => s.Metadata.ShellUse == "Message window");
            return factory.CreateExport().Value;
        }

        public void ShowShell(string shellUse)
        {
            var factory = shellFactories.Single(s => s.Metadata.ShellUse == shellUse);
            var context = factory.CreateExport();
            var owner = mainWindow as System.Windows.Window;
            if (owner != null)
                context.Value.Owner = owner;
            context.Value.Show();
        }


        public IShell GetShell(string shellUse)
        {
            var factory = shellFactories.Single(s => s.Metadata.ShellUse == shellUse);
            var context = factory.CreateExport();
            var owner = mainWindow as System.Windows.Window;
            if (owner != null)
                context.Value.Owner = owner;
            return context.Value;
        }

        public IShell GetMainShell()
        {
            return mainWindow;
        }

        public System.Windows.Forms.FolderBrowserDialog CreateFolderBrowserDialog()
        {
            return new System.Windows.Forms.FolderBrowserDialog();
        }

        public void FirstRunWindowSetup()
        {
            if (appSettingsService.Settings.FirstRun)
            {
                SetInitialWindowDimensions();
                var settings = appSettingsService.Settings;
                settings.FirstRun = false;
                appSettingsService.SaveChanges(settings);
            }
        }

        private void SetInitialWindowDimensions()
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double left, top, width, height;
            var widthRatio = (bounds.Width / measureProvider.ScaleX) * 0.1d;
            var heightRatio = (bounds.Height / measureProvider.ScaleY) * 0.1d;
            left = bounds.Left / measureProvider.ScaleX + widthRatio;
            top = bounds.Top / measureProvider.ScaleY + heightRatio;
            width = bounds.Width / measureProvider.ScaleX - widthRatio * 2;
            height = bounds.Height / measureProvider.ScaleY - heightRatio * 2;
            mainWindow.Left = double.IsInfinity(left) ? 0 : left;
            mainWindow.Top = double.IsInfinity(top) ? 0 : top;
            mainWindow.Width = double.IsInfinity(width) ? bounds.Width : width;
            mainWindow.Height = double.IsInfinity(height) ? bounds.Height : height;
        }


        public void OpenMainWindow()
        {
            if (CanOpenMainWindow())
            {
                if (mainWindow == null)
                {
                    var mainWindowFactory = shellFactories.Single(s => s.Metadata.ShellUse == "Main window");
                    var context = mainWindowFactory.CreateExport();
                    mainWindow = context.Value;
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

        public void CloseMainWindow()
        {
            if (mainWindow != null)
            {
                mainWindow.Close();
            }
        }


        public void Shutdown()
        {
            CloseMainWindow();
            trayIcon.Dispose();
        }


        private void ShowMainWindow()
        {
            mainWindow.Closing += (s, e) => SaveWindowPosition();
            LoadWindowPosition();
            mainWindow.Show();
        }

        private bool CanOpenMainWindow()
        {
            if (appSettingsService.Settings.IsMasterPasswordSet)
            {
                var passwordWindowFactory = shellFactories.Single(s => s.Metadata.ShellUse == "Password window");
                var context = passwordWindowFactory.CreateExport();
                var passwordWindow = context.Value;
                bool? dialog = passwordWindow.ShowDialog();
                if (dialog.HasValue)
                {
                    return dialog.Value;
                }
                return false;
            }
            else
                return true;
        }

        private void LoadWindowPosition()
        {
            var mainWindowSettings = userSettingsService.MainWindowSettings;
            mainWindow.Left = mainWindowSettings.Left;
            mainWindow.Top = mainWindowSettings.Top;
            mainWindow.Width = mainWindowSettings.Width;
            mainWindow.Height = mainWindowSettings.Height;
        }


        private void SaveWindowPosition()
        {
            userSettingsService.MainWindowSettings.Height = mainWindow.Height;
            userSettingsService.MainWindowSettings.Width = mainWindow.Width;
            userSettingsService.MainWindowSettings.Left = mainWindow.Left;
            userSettingsService.MainWindowSettings.Top = mainWindow.Top;
            mainWindow = null;
        }
    }
}

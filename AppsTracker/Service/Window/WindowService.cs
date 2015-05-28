using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Views;
using AppsTracker.Widgets;

namespace AppsTracker.Data.Service
{
    [Export(typeof(IWindowService))]
    public sealed class WindowService : IWindowService
    {
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ITrayIcon trayIcon;
        private readonly IEnumerable<ExportFactory<IShell, IShellMetaData>> shellFactories;

        private readonly MeasureProvider measureProvider;

        private IShell mainWindow;
        private IShell limitNotifyWindow;

        [ImportingConstructor]
        public WindowService(ISqlSettingsService sqlSettingsService,
                             IXmlSettingsService xmlSettingsService,
                             ITrayIcon trayIcon,
                             [ImportMany]IEnumerable<ExportFactory<IShell, IShellMetaData>> shellFactories,
                             MeasureProvider measureProvider)
        {
            this.sqlSettingsService = sqlSettingsService;
            this.xmlSettingsService = xmlSettingsService;
            this.trayIcon = trayIcon;
            this.shellFactories = shellFactories;
            this.measureProvider = measureProvider;

            limitNotifyWindow = GetShell("Limit toast window");
            limitNotifyWindow.Show();
        }

        public void ShowMessageDialog(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.ShowDialog();
        }

        public void ShowMessageDialog(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.ShowDialog();
        }

        public void ShowMessage(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.Show();
        }

        public void ShowMessage(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.Show();
        }

        public void ShowShell(string shellUse)
        {
            var factory = shellFactories.Single(s => s.Metadata.ShellUse == shellUse);
            using (var context = factory.CreateExport())
            {
                context.Value.Show();
            }
        }


        public IShell GetShell(string shellUse)
        {
            var factory = shellFactories.Single(s => s.Metadata.ShellUse == shellUse);
            using (var context = factory.CreateExport())
            {
                return context.Value;
            }
        }

        public System.Windows.Forms.FolderBrowserDialog CreateFolderBrowserDialog()
        {
            return new System.Windows.Forms.FolderBrowserDialog();
        }

        public void FirstRunWindowSetup()
        {
            if (sqlSettingsService.Settings.FirstRun)
            {
                SetInitialWindowDimensions();
                var settings = sqlSettingsService.Settings;
                settings.FirstRun = false;
                sqlSettingsService.SaveChanges(settings);
            }
        }

        private void SetInitialWindowDimensions()
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double left, top, width, height;
            var widthRatio = bounds.Width / measureProvider.ScaleX * 0.15d;
            var heightRatio = bounds.Height / measureProvider.ScaleY * 0.15d;
            left = bounds.Left / measureProvider.ScaleX + widthRatio;
            top = bounds.Top / measureProvider.ScaleY + heightRatio;
            width = bounds.Width / measureProvider.ScaleX - widthRatio * 2;
            height = bounds.Height / measureProvider.ScaleY - heightRatio * 2;
            mainWindow.Left = left;
            mainWindow.Top = top;
            mainWindow.Width = width;
            mainWindow.Height = height;
        }


        public void InitializeTrayIcon()
        {
            trayIcon.ShowApp.Click += (s, e) => CreateOrShowMainWindow();
            trayIcon.IsVisible = true;
        }

        public void CreateOrShowMainWindow()
        {
            if (CanOpenMainWindow())
            {
                if (mainWindow == null)
                {
                    var mainWindowFactory = shellFactories.Single(s => s.Metadata.ShellUse == "Main window");
                    using (var context = mainWindowFactory.CreateExport())
                    {
                        mainWindow = context.Value;
                    }
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
            if (sqlSettingsService.Settings.IsMasterPasswordSet)
            {
                IShell passwordWindow;
                var passwordWindowFactory = shellFactories.Single(s => s.Metadata.ShellUse == "Password window");
                using (var context = passwordWindowFactory.CreateExport())
                {
                    passwordWindow = context.Value;
                }
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


        private void SaveWindowPosition()
        {
            xmlSettingsService.MainWindowSettings.Height = mainWindow.Height;
            xmlSettingsService.MainWindowSettings.Width = mainWindow.Width;
            xmlSettingsService.MainWindowSettings.Left = mainWindow.Left;
            xmlSettingsService.MainWindowSettings.Top = mainWindow.Top;
            mainWindow = null;
        }
    }
}

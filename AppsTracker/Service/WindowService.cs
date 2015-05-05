using System;
using System.ComponentModel.Composition;
using AppsTracker.Widgets;

namespace AppsTracker.Service
{
    [Export(typeof(IWindowService))]
    public sealed class WindowService : IWindowService
    {
        private readonly ISqlSettingsService sqlSettingsService;
        private readonly IXmlSettingsService xmlSettingsService;
        private readonly ITrayIcon trayIcon;
        private readonly ExportFactory<IShell> mainWindowValueFactory;
        private readonly ExportFactory<IPasswordWindow> passwordWindowValueFactory;

        private IShell mainWindow;     


        [ImportingConstructor]
        public WindowService(ISqlSettingsService sqlSettingsService, IXmlSettingsService xmlSettingsService,
                             ITrayIcon trayIcon, ExportFactory<IShell> mainWindowValueFactory,
                             ExportFactory<IPasswordWindow> passwordWindowValueFactory)
        {
            this.sqlSettingsService = sqlSettingsService;
            this.xmlSettingsService = xmlSettingsService;
            this.trayIcon = trayIcon;
            this.mainWindowValueFactory = mainWindowValueFactory;
            this.passwordWindowValueFactory = passwordWindowValueFactory;
        }


        public void ShowDialog(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.ShowDialog();
        }

        public void ShowDialog(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.ShowDialog();
        }

        public void Show(string message, bool showCancel = true)
        {
            var msgWindow = new MessageWindow(message, showCancel);
            msgWindow.Show();
        }

        public void Show(Exception fail)
        {
            var msgWindow = new MessageWindow(fail);
            msgWindow.Show();
        }


        public void ShowWindow<T>() where T : System.Windows.Window
        {
            var instance = Activator.CreateInstance<T>();            
            instance.Show();
        }


        public void ShowWindow<T>(params object[] args) where T : System.Windows.Window
        {
            T instance = (T)Activator.CreateInstance(typeof(T), args);
            instance.Show();
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
    }
}

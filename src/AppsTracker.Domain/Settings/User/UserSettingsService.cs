#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Linq;
using AppsTracker.Data.XmlSettings;

namespace AppsTracker.Data.Repository
{
    [Export(typeof(IUserSettingsService))]
    public sealed class UserSettingsService : IUserSettingsService
    {
        private const string SETTINGS_FILE_NAME = "settings.xml";
        private readonly string settingsPath = Path.Combine(Environment.GetFolderPath
            (Environment.SpecialFolder.CommonApplicationData), "AppService");


        public AppSettings AppSettings
        {
            get;
            private set;
        }


        public LogsViewSettings LogsViewSettings
        {
            get;
            private set;
        }


        public ScreenshotsViewSettings ScreenshotsViewSettings
        {
            get;
            private set;
        }

        public DaysViewSettings DaysViewSettings
        {
            get;
            private set;
        }


        public MainWindowSettings MainWindowSettings
        {
            get;
            private set;
        }


        public LimitsSettings LimitsSettings
        {
            get;
            private set;
        }


        public void Initialize()
        {
            LimitsSettings = new LimitsSettings();
            AppSettings = new AppSettings();
            LogsViewSettings = new LogsViewSettings();
            ScreenshotsViewSettings = new ScreenshotsViewSettings();
            DaysViewSettings = new DaysViewSettings();
            MainWindowSettings = new MainWindowSettings();

            if (!File.Exists(Path.Combine(settingsPath, SETTINGS_FILE_NAME)))
                return;

            var xml = XDocument.Load(Path.Combine(settingsPath, SETTINGS_FILE_NAME));
            var root = xml.Element("root");

            SetAppSettingsValues(root);
            SetLogsViewSettingsValues(root);
            SetScreenshotViewSettingsValues(root);
            SetDaysViewSettingsValues(root);
            SetMainWindowSettingsValues(root);

        }

        private void SetAppSettingsValues(XElement root)
        {
            try
            {
                var node = root.Element("AppSettings");
                AppSettings.SetValues(node);
            }
            catch
            {
                AppSettings = new AppSettings();
            }
        }

        private void SetLogsViewSettingsValues(XElement root)
        {
            try
            {
                var node = root.Element("LogsViewSettings");
                LogsViewSettings.SetValues(node);
            }
            catch
            {
                LogsViewSettings = new LogsViewSettings();
            }
        }

        private void SetScreenshotViewSettingsValues(XElement root)
        {
            try
            {
                var node = root.Element("ScreenshotsViewSettings");
                ScreenshotsViewSettings.SetValues(node);
            }
            catch (Exception)
            {
                ScreenshotsViewSettings = new ScreenshotsViewSettings();
            }
        }

        private void SetDaysViewSettingsValues(XElement root)
        {
            try
            {
                var node = root.Element("DaysViewSettings");
                DaysViewSettings.SetValues(node);
            }
            catch
            {
                DaysViewSettings = new DaysViewSettings();
            }
        }

        private void SetMainWindowSettingsValues(XElement root)
        {
            try
            {
                var node = root.Element("MainWindowSettings");
                MainWindowSettings.SetValues(node);
            }
            catch
            {
                MainWindowSettings = new MainWindowSettings();
            }
        }

        public void Shutdown()
        {
            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            var xml = new XElement("root");
            xml.Add(AppSettings.GetXML());
            xml.Add(LogsViewSettings.GetXML());
            xml.Add(ScreenshotsViewSettings.GetXML());
            xml.Add(DaysViewSettings.GetXML());
            xml.Add(MainWindowSettings.GetXML());

            xml.Save(Path.Combine(settingsPath, SETTINGS_FILE_NAME), SaveOptions.None);
        }
    }
}

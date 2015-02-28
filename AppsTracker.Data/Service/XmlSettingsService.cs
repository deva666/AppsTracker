using System;
using System.IO;
using System.Xml.Linq;
using AppsTracker.Data.XmlSettings;

namespace AppsTracker.Data.Service
{
    public sealed class XmlSettingsService : IXmlSettingsService
    {
        private const string SETTINGS_FILE_NAME = "settings.xml";
        private readonly string settingsPath = Path.Combine(Environment.GetFolderPath
            (Environment.SpecialFolder.CommonApplicationData), "AppService");

        private static readonly Lazy<XmlSettingsService> instance =
            new Lazy<XmlSettingsService>(() => new XmlSettingsService());
        public static XmlSettingsService Instance { get { return instance.Value; } }

        public LogsViewSettings LogsViewSettings { get; private set; }
        public KeylogsViewSettings KeylogsViewSettings { get; private set; }
        public ScreenshotsViewSettings ScreenshotsViewSettings { get; private set; }
        public DaysViewSettings DaysViewSettings { get; private set; }
        public MainWindowSettings MainWindowSettings { get; private set; }

        private XmlSettingsService() { }

        public void Initialize()
        {
            if (File.Exists(Path.Combine(settingsPath, SETTINGS_FILE_NAME)) == false)
                return;

            var xml = XDocument.Load(Path.Combine(settingsPath, SETTINGS_FILE_NAME));
            var root = xml.Element("root");

            TrySetLogsViewSettings(root);
            TrySetKeylogsViewValues(root);
            TrySetScreenshotsViewValues(root);
            TrySetDaysViewSettings(root);
            TrySetMainWindowSettings(root);
        }

        private void TrySetLogsViewSettings(XElement root)
        {
            LogsViewSettings = new LogsViewSettings();

            var node = root.Element(LogsViewSettings.GetType().Name);
            if (node == null)
                return;

            try
            {
                LogsViewSettings.SetValues(node);
            }
            catch
            {
                LogsViewSettings = new LogsViewSettings();
            }
        }

        private void TrySetKeylogsViewValues(XElement root)
        {
            KeylogsViewSettings = new KeylogsViewSettings();

            var node = root.Element(KeylogsViewSettings.GetType().Name);
            if (node == null)
                return;

            try
            {
                KeylogsViewSettings.SetValues(node);
            }
            catch
            {
                KeylogsViewSettings = new KeylogsViewSettings();
            }
        }

        private void TrySetScreenshotsViewValues(XElement root)
        {
            ScreenshotsViewSettings = new ScreenshotsViewSettings();

            var node = root.Element(ScreenshotsViewSettings.GetType().Name);
            if (node == null)
                return;

            try
            {
                ScreenshotsViewSettings.SetValues(node);
            }
            catch
            {
                ScreenshotsViewSettings = new ScreenshotsViewSettings();
            }
        }

        private void TrySetDaysViewSettings(XElement root)
        {
            DaysViewSettings = new DaysViewSettings();

            var node = root.Element(DaysViewSettings.GetType().Name);
            if (node == null)
                return;

            try
            {
                DaysViewSettings.SetValues(node);
            }
            catch
            {
                DaysViewSettings = new DaysViewSettings();
            }
        }

        private void TrySetMainWindowSettings(XElement root)
        {
            MainWindowSettings = new MainWindowSettings();

            var node = root.Element(MainWindowSettings.GetType().Name);
            if (node == null)
                return;

            try
            {
                MainWindowSettings.SetValues(node);
            }
            catch
            {
                MainWindowSettings = new MainWindowSettings();
            }
        }

        public void ShutDown()
        {
            if (Directory.Exists(settingsPath) == false)
                Directory.CreateDirectory(settingsPath);

            var xml = new XElement("root");
            xml.Add(LogsViewSettings.GetXML());
            xml.Add(KeylogsViewSettings.GetXML());
            xml.Add(ScreenshotsViewSettings.GetXML());
            xml.Add(DaysViewSettings.GetXML());
            xml.Add(MainWindowSettings.GetXML());

            xml.Save(Path.Combine(settingsPath, SETTINGS_FILE_NAME), SaveOptions.None);
        }
    }
}

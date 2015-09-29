#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml.Linq;
using AppsTracker.Data.XmlSettings;

namespace AppsTracker.Data.Service
{
    [Export(typeof(IXmlSettingsService))]
    public sealed class XmlSettingsService : IXmlSettingsService
    {
        private const string SETTINGS_FILE_NAME = "settings.xml";
        private readonly string settingsPath = Path.Combine(Environment.GetFolderPath
            (Environment.SpecialFolder.CommonApplicationData), "AppService");

        private readonly IList<XmlSettingsBase> settingsContainer = new List<XmlSettingsBase>();

        private readonly AppSettings appSettings = new AppSettings();

        public AppSettings AppSettings
        {
            get { return appSettings; }
        }

        private readonly LogsViewSettings logsViewSettings = new LogsViewSettings();

        public LogsViewSettings LogsViewSettings
        {
            get { return logsViewSettings; }
        }


        private readonly KeylogsViewSettings keylogsViewSettings = new KeylogsViewSettings();

        public KeylogsViewSettings KeylogsViewSettings
        {
            get { return keylogsViewSettings; }
        }

        private readonly ScreenshotsViewSettings screenshotsViewSettings = new ScreenshotsViewSettings();
        
        public ScreenshotsViewSettings ScreenshotsViewSettings
        {
            get { return screenshotsViewSettings; }
        }

        private readonly DaysViewSettings daysViewSettings = new DaysViewSettings();

        public DaysViewSettings DaysViewSettings
        {
            get { return daysViewSettings; }
        }

        private readonly MainWindowSettings mainWindowSettings = new MainWindowSettings();

        public MainWindowSettings MainWindowSettings
        {
            get { return mainWindowSettings; }
        }

        private  LimitsSettings limitsSettings;

        public LimitsSettings LimitsSettings
        {
            get { return limitsSettings ?? (limitsSettings = new LimitsSettings()); }
        }


        public void Initialize()
        {
            settingsContainer.Add(appSettings);
            settingsContainer.Add(logsViewSettings);
            settingsContainer.Add(keylogsViewSettings);
            settingsContainer.Add(screenshotsViewSettings);
            settingsContainer.Add(daysViewSettings);
            settingsContainer.Add(mainWindowSettings);

            if (File.Exists(Path.Combine(settingsPath, SETTINGS_FILE_NAME)) == false)
                return;

            var xml = XDocument.Load(Path.Combine(settingsPath, SETTINGS_FILE_NAME));
            var root = xml.Element("root");

            TryGetValuesFromXml(settingsContainer, root);
        }

        private void TryGetValuesFromXml(IEnumerable<XmlSettingsBase> settings, XElement root)
        {
            foreach (var settingsBase in settings)
            {
                var node = root.Element(settingsBase.GetType().Name);
                if (node == null)
                    return;

                try
                {
                    settingsBase.SetValues(node);
                }
                catch
                {
                    SetDefaultPropertyValue(settingsBase.GetType().Name, settingsBase.GetType());
                }
            }
        }


        private void SetDefaultPropertyValue(string propertyName, Type propertyType)
        {
            var property = this.GetType().GetProperty(propertyName);
            var newInstance = Activator.CreateInstance(propertyType);
            property.SetValue(this, newInstance);
        }


        public void ShutDown()
        {
            if (Directory.Exists(settingsPath) == false)
                Directory.CreateDirectory(settingsPath);

            var xml = new XElement("root");
            xml.Add(appSettings.GetXML());
            xml.Add(logsViewSettings.GetXML());
            xml.Add(keylogsViewSettings.GetXML());
            xml.Add(screenshotsViewSettings.GetXML());
            xml.Add(daysViewSettings.GetXML());
            xml.Add(mainWindowSettings.GetXML());

            xml.Save(Path.Combine(settingsPath, SETTINGS_FILE_NAME), SaveOptions.None);
        }
    }
}

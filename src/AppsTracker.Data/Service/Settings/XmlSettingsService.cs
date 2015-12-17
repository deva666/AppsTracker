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

            var settingsContainer = new List<XmlSettingsBase>();
            settingsContainer.Add(AppSettings = new AppSettings());
            settingsContainer.Add(LogsViewSettings = new LogsViewSettings());
            settingsContainer.Add(ScreenshotsViewSettings = new ScreenshotsViewSettings());
            settingsContainer.Add(DaysViewSettings = new DaysViewSettings());
            settingsContainer.Add(MainWindowSettings = new MainWindowSettings());

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
            var property = GetType().GetProperty(propertyName);
            var newInstance = Activator.CreateInstance(propertyType);
            if (property.CanWrite)
            {
                property.SetValue(this, newInstance);
            }
        }


        public void ShutDown()
        {
            if (Directory.Exists(settingsPath) == false)
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

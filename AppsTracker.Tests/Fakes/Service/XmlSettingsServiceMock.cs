using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(IXmlSettingsService))]
    class XmlSettingsServiceMock : IXmlSettingsService
    {
        public Data.XmlSettings.LogsViewSettings LogsViewSettings
        {
            get { return new Data.XmlSettings.LogsViewSettings(); }
        }

        public Data.XmlSettings.KeylogsViewSettings KeylogsViewSettings
        {
            get { return new Data.XmlSettings.KeylogsViewSettings(); }
        }

        public Data.XmlSettings.ScreenshotsViewSettings ScreenshotsViewSettings
        {
            get { return new Data.XmlSettings.ScreenshotsViewSettings(); }
        }

        public Data.XmlSettings.DaysViewSettings DaysViewSettings
        {
            get { return new Data.XmlSettings.DaysViewSettings(); }
        }

        public Data.XmlSettings.MainWindowSettings MainWindowSettings
        {
            get { return new Data.XmlSettings.MainWindowSettings(); }
        }

        public void Initialize()
        {

        }

        public void ShutDown()
        {

        }


        public Data.XmlSettings.LimitsSettings LimitsSettings
        {
            get { return new Data.XmlSettings.LimitsSettings(); }
        }
    }
}

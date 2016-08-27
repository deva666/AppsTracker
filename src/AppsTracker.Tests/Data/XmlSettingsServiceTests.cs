using System;
using AppsTracker.Data.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Data
{
    [TestClass]
    public class XmlSettingsServiceTests
    {
        [TestMethod]
        public void TestInitialize()
        {
            var settingsService = new XmlSettingsService();
            settingsService.Initialize();

            Assert.IsNotNull(settingsService.AppSettings);
            Assert.IsNotNull(settingsService.DaysViewSettings);
            Assert.IsNotNull(settingsService.LimitsSettings);
            Assert.IsNotNull(settingsService.LogsViewSettings);
            Assert.IsNotNull(settingsService.MainWindowSettings);
            Assert.IsNotNull(settingsService.ScreenshotsViewSettings);
        }
    }
}

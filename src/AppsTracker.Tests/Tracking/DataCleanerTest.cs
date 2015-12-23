using System;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;
using AppsTracker.Tracking.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class DataCleanerTest
    {
        [TestMethod]
        public void TestInitializeCleaner()
        {
            var logCleaner = new Mock<ILogCleaner>();
            var cleaner = new DataCleaner(logCleaner.Object);
            var settings = new Setting() { DeleteOldLogs = true, OldLogDeleteDays = 20 };

            cleaner.Initialize(settings);
            logCleaner.Verify(l => l.Clean(), Times.Once());
        }

        [TestMethod]
        public void TestDisabledCleaner()
        {
            var logCleaner = new Mock<ILogCleaner>();
            var cleaner = new DataCleaner(logCleaner.Object);
            var settings = new Setting() { DeleteOldLogs = false, OldLogDeleteDays = 20 };

            cleaner.Initialize(settings);
            logCleaner.Verify(l => l.Clean(), Times.Never());
        }
    }
}

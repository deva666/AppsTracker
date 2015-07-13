using System;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class WindowTrackerTest : TestMockBase
    {
        [TestMethod]
        public void TestChangeApp()
        {
            var tracker = CreateTracker();

            trackingService.Setup(t => t.CreateLogEntryAsync(It.IsAny<LogInfo>()))
                .Returns(Task.FromResult(new Log()));


        }

        private WindowTracker CreateTracker()
        {
            return new WindowTracker(trackingService.Object,
                                     dataService.Object,
                                     appChangedNotifier.Object,
                                     screenshotTracker.Object,
                                     syncContext,
                                     mediator);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class ScreenshotTrackerTest : TestMockBase
    {
        [TestMethod]
        public async Task TestTakeScreenshot()
        {
            var tracker = CreateTracker();
            var settings = new Setting()
            {
                TimerInterval = 500,
                TrackingEnabled = true,
                TakeScreenshots = true
            };
            var eventRaised = 0;
            var screenshot = new Screenshot() { ScreenshotID = 10 };

            tracker.ScreenshotTaken += (s, e) =>
            {
                Interlocked.Exchange(ref eventRaised, 1);
                Assert.AreSame(screenshot, e.Screenshot);
            };

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(700);

            Assert.AreEqual(1, eventRaised, "Screenshot taken event should be raised");
        }

        [TestMethod]
        public async Task TestDisabledScreenshots()
        {
            var tracker = CreateTracker();
            var settings = new Setting()
            {
                TimerInterval = 100,
                TrackingEnabled = true,
                TakeScreenshots = false
            };
            var screenshot = new Screenshot() { ScreenshotID = 10 };

            tracker.ScreenshotTaken += (s, e) =>
            {
                Assert.Fail("Should not be called if screenshots are disabled");
            };

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(200);
        }

        [TestMethod]
        public async Task TestDisabledTracking()
        {
            var tracker = CreateTracker();
            var settings = new Setting()
            {
                TimerInterval = 100,
                TrackingEnabled = false,
                TakeScreenshots = true
            };
            var screenshot = new Screenshot() { ScreenshotID = 10 };

            tracker.ScreenshotTaken += (s, e) =>
            {
                Assert.Fail("Should not be called if tracking is disabled");
            };

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(200);
        }

        private ScreenshotTracker CreateTracker()
        {
            return new ScreenshotTracker(screenshotFactory.Object,
                                         syncContext,
                                         dataService.Object);
        }
    }
}

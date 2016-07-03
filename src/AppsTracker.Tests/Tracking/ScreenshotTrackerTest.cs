using System;
using System.Reactive.Linq;
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
                TimerInterval = 100,
                TrackingEnabled = true,
                TakeScreenshots = true
            };
            var observableCalled = 0;
            var screenshot = new Screenshot() { ScreenshotID = 10 };

            tracker.ScreenshotObservable.Subscribe(s => Interlocked.Exchange(ref observableCalled, 1));

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(150);

            Assert.AreEqual(1, observableCalled, "Screenshot taken event should be raised");
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

            tracker.ScreenshotObservable.Subscribe(s => Assert.Fail("Should not be called if screenshots are disabled"));

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(150);
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

            tracker.ScreenshotObservable.Subscribe(s => Assert.Fail("Should not be called if tracking is disabled"));

            screenshotFactory.Setup(f => f.CreateScreenshot()).Returns(screenshot);
            tracker.Initialize(settings);

            await Task.Delay(150);
        }

        private ScreenshotTracker CreateTracker()
        {
            return new ScreenshotTracker(screenshotFactory.Object,
                                         syncContext,
                                         dataService.Object);
        }
    }
}

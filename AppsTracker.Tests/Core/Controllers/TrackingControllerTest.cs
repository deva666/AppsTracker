using AppsTracker.Controllers;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.Controllers
{
    [TestClass]
    public class TrackingControllerTest
    {
        [TestMethod]
        public void TestModuleInitialization()
        {
            var settings = new Setting() { TrackingEnabled = true };
            var module = new Mock<ITrackingModule>();
            var trackingController = new TrackingController(new ITrackingModule[] { module.Object });
            trackingController.Initialize(settings);
            module.Verify(m => m.Initialize(settings), Times.Once);
        }

        [TestMethod]
        public void TestSettingsChanging()
        {
            var settings = new Setting() { TrackingEnabled = true };
            var module = new Mock<ITrackingModule>();
            var trackingController = new TrackingController(new ITrackingModule[] { module.Object });
            trackingController.SettingsChanging(settings);
            module.Verify(m => m.SettingsChanged(settings), Times.Once);
        }

        [TestMethod]
        public void TestDispose()
        {
            var module = new Mock<ITrackingModule>();
            var trackingController = new TrackingController(new ITrackingModule[] { module.Object });
            trackingController.Dispose();
            module.Verify(m => m.Dispose(), Times.Once);
        }
    }
}

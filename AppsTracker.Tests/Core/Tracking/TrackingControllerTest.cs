using System;
using AppsTracker.Controllers;
using AppsTracker.Data.Models;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.Tracking
{
    [TestClass]
    public class TrackingControllerTest
    {
        [TestMethod]
        public void TestModuleInitialization()
        {
            var settings = new Setting() { LoggingEnabled = true };
            var module = new Mock<IModule>();
            var trackingController = new TrackingController(new IModule[] { module.Object });
            trackingController.Initialize(settings);
            module.Verify(m => m.InitializeComponent(settings), Times.Once);
        }

        [TestMethod]
        public void TestSettingsChanging()
        {
            var settings = new Setting() { LoggingEnabled = true };
            var module = new Mock<IModule>();
            var trackingController = new TrackingController(new IModule[] { module.Object });
            trackingController.SettingsChanging(settings);
            module.Verify(m => m.SettingsChanged(settings), Times.Once);
        }

        [TestMethod]
        public void TestDispose()
        {
            var module = new Mock<IModule>();
            var trackingController = new TrackingController(new IModule[] { module.Object });
            trackingController.Dispose();
            module.Verify(m => m.Dispose(), Times.Once);   
        }
    }
}

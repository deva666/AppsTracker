using System;
using AppsTracker.Controllers;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.Tracking
{
    [TestClass]
    public class TrackingControllerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            
        }

        [TestMethod]
        public void TestMethod1()
        {
            var module = new Mock<IModule>();
            var trackingController = new TrackingController();

        }
    }
}

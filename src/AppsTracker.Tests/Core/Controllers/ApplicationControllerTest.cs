using AppsTracker.Controllers;
using AppsTracker.Data.Models;
using AppsTracker.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.Controllers
{
    [TestClass]
    public class ApplicationControllerTest : TestMockBase
    {
        [TestMethod]
        public void TestInitializeWithAutostart()
        {
            settingsService.SetupGet(s => s.Settings).Returns(new Setting());

            var controller = CreateController();
            controller.Initialize(true);

            xmlSettingsService.Verify(x => x.Initialize(), Times.Once());
            appearanceController.Verify(a => a.Initialize(It.IsAny<Setting>()), Times.Once());
            windowService.Verify(w => w.OpenMainWindow(), Times.Never());
            windowService.Verify(w => w.FirstRunWindowSetup(), Times.Never());
        }

        [TestMethod]
        public void TestInitializeWithoutAutostart()
        {
            settingsService.SetupGet(s => s.Settings).Returns(new Setting());

            var controller = CreateController();
            controller.Initialize(false);

            xmlSettingsService.Verify(x => x.Initialize(), Times.Once());
            appearanceController.Verify(a => a.Initialize(It.IsAny<Setting>()), Times.Once());
            windowService.Verify(w => w.OpenMainWindow(), Times.Once());
            windowService.Verify(w => w.FirstRunWindowSetup(), Times.Once());
        }


        [TestMethod]
        public void TestShutdown()
        {
            var controller = CreateController();
            controller.Shutdown();

            windowService.Verify(w => w.Shutdown(), Times.Once());
            xmlSettingsService.Verify(x => x.Shutdown(), Times.Once());
            trackingController.Verify(t => t.Shutdown(), Times.Once());
        }

        private ApplicationController CreateController()
        {
            return new ApplicationController(appearanceController.Object,
                                            trackingController.Object,
                                            settingsService.Object,
                                            xmlSettingsService.Object,
                                            repository.Object,
                                            windowService.Object);
        }
    }
}

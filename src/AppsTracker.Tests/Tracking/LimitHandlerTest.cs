using AppsTracker.Common.Communication;
using AppsTracker.Common.Logging;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class LimitHandlerTest
    {
        [TestMethod]
        public void TestShowWarning()
        {
            var xmlSettings = new XmlSettingsService();
            xmlSettings.Initialize();

            var logger = new Mock<ILogger>();
            var mediator = new Mock<IMediator>();
            var shutdownService = new Mock<IShutdownService>();

            var limitHandler = new LimitHandler(mediator.Object, xmlSettings, logger.Object, shutdownService.Object);
            var app = new Aplication() { WinName = "test app" };
            var limit = new AppLimit() { LimitReachedAction = LimitReachedAction.Warn, Application = app };

            limitHandler.Handle(limit);
            mediator.Verify(m => m.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit), Times.Once());
            shutdownService.Verify(s => s.Shutdown(app.WinName), Times.Never());
        }

        [TestMethod]
        public void TestShutdownApp()
        {
            var xmlSettings = new XmlSettingsService();
            xmlSettings.Initialize();

            var logger = new Mock<ILogger>();
            var mediator = new Mock<IMediator>();
            var shutdownService = new Mock<IShutdownService>();

            var limitHandler = new LimitHandler(mediator.Object, xmlSettings, logger.Object, shutdownService.Object);
            var app = new Aplication() { WinName = "test app" };
            var limit = new AppLimit() { LimitReachedAction = LimitReachedAction.Shutdown, Application = app };

            limitHandler.Handle(limit);
            mediator.Verify(m => m.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit), Times.Never());
            shutdownService.Verify(s => s.Shutdown(app.WinName), Times.Once());
        }

        [TestMethod]
        public void TestShutdownAndWarn()
        {
            var xmlSettings = new XmlSettingsService();
            xmlSettings.Initialize();

            var logger = new Mock<ILogger>();
            var mediator = new Mock<IMediator>();
            var shutdownService = new Mock<IShutdownService>();

            var limitHandler = new LimitHandler(mediator.Object, xmlSettings, logger.Object, shutdownService.Object);
            var app = new Aplication() { WinName = "test app" };
            var limit = new AppLimit() { LimitReachedAction = LimitReachedAction.WarnAndShutdown, Application = app };

            limitHandler.Handle(limit);
            mediator.Verify(m => m.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit), Times.Once());
            shutdownService.Verify(s => s.Shutdown(app.WinName), Times.Once());
        }

        [TestMethod]
        public void TestLimitDisabled()
        {
            var xmlSettings = new XmlSettingsService();
            xmlSettings.Initialize();

            var logger = new Mock<ILogger>();
            var mediator = new Mock<IMediator>();
            var shutdownService = new Mock<IShutdownService>();

            var limitHandler = new LimitHandler(mediator.Object, xmlSettings, logger.Object, shutdownService.Object);
            var app = new Aplication() { WinName = "test app" };
            var limit = new AppLimit() { LimitReachedAction = LimitReachedAction.Shutdown, Application = app };
            xmlSettings.LimitsSettings.DontShowLimits.Add(limit);

            limitHandler.Handle(limit);
            mediator.Verify(m => m.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit), Times.Never());
        }
    }
}

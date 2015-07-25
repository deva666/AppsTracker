using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Hooks;
using AppsTracker.Tracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AppsTracker.Tests.Fakes;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class LimitObserverTest : TestMockBase
    {
        [TestMethod]
        public void TestImmediateLimitHandle()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(0);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            trackingService.Setup(t => t.GetDayDuration(app)).Returns(0);
            trackingService.Setup(t => t.GetApp(appInfo, 0)).Returns(app);

            observer.Initialize(null);
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }


        [TestMethod]
        public async Task TestDelayedLimitHandle()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(1000);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            trackingService.Setup(t => t.GetDayDuration(app)).Returns(0);
            trackingService.Setup(t => t.GetApp(appInfo, 0)).Returns(app);

            observer.Initialize(null);
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);

            await Task.Delay(2000);

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }

        [TestMethod]
        public async Task TestMidnightLimitsReset()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(1000);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            trackingService.Setup(t => t.GetDayDuration(app)).Returns(0);
            trackingService.Setup(t => t.GetApp(appInfo, 0)).Returns(app);

            observer.Initialize(null);
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);

            await Task.Delay(700);
            midnightNotifier.Raise(m => m.MidnightTick += null, EventArgs.Empty);
            await Task.Delay(700);
            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);
            await Task.Delay(500);
            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }

        private LimitObserver CreateObserver()
        {
            return new LimitObserver(trackingService.Object,
                                     dataService.Object,
                                     windowChangedNotifier.Object,
                                     midnightNotifier.Object,
                                     limitHandler.Object,
                                     mediator,
                                     new WorkQueueMock());
        }

        private Aplication CreateAppDailyLimit(int limitAmount)
        {
            var app = new Aplication();
            app.ApplicationID = 7;
            app.Name = "some app";

            var limit = new AppLimit();
            limit.ApplicationID = 7;
            limit.Application = app;
            limit.Limit = TimeSpan.FromMilliseconds(limitAmount).Ticks;
            limit.LimitReachedAction = LimitReachedAction.Warn;
            limit.LimitSpan = LimitSpan.Day;

            app.Limits = new List<AppLimit>() { limit };
            return app;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Hooks;
using AppsTracker.Tracking.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class LimitObserverTest : TestMockBase
    {
        [TestMethod]
        public async Task TestImmediateLimitHandle()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(0);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            appDurationCalc.Setup(t => t.GetDuration(app, LimitSpan.Day)).ReturnsAsync(0);
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>())).ReturnsAsync(new List<Aplication>() { app });

            observer.Initialize(new Setting() { TrackingEnabled = true });
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            await Task.Delay(100);

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }

        [TestMethod]
        public async Task TestDisabledObserver()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(0);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))

                .Returns(new List<Aplication> { app });
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>())).ReturnsAsync(new List<Aplication>() { app });
            appDurationCalc.Setup(t => t.GetDuration(app, LimitSpan.Day)).ReturnsAsync(0);

            observer.Initialize(new Setting() { TrackingEnabled = false });
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            await Task.Delay(100);

            limitHandler.Verify(h => h.Handle(It.IsAny<AppLimit>()), Times.Never());
        }

        [TestMethod]
        public async Task TestDelayedLimitHandle()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(100);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>())).ReturnsAsync(new List<Aplication>() { app });
            appDurationCalc.Setup(t => t.GetDuration(app, LimitSpan.Day)).ReturnsAsync(0);

            observer.Initialize(new Setting() { TrackingEnabled = true });
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);

            await Task.Delay(200);

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }

        [TestMethod]
        public async Task TestMidnightLimitsReset()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(500);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>())).ReturnsAsync(new List<Aplication>() { app });
            appDurationCalc.Setup(t => t.GetDuration(app, LimitSpan.Day)).ReturnsAsync(0);

            observer.Initialize(new Setting() { TrackingEnabled = true });
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);

            await Task.Delay(200);
            midnightNotifier.Raise(m => m.MidnightTick += null, EventArgs.Empty);
            await Task.Delay(400);
            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Never);
            await Task.Delay(500);
            limitHandler.Verify(h => h.Handle(app.Limits.First()), Times.Once);
        }

        [TestMethod]
        public async Task TestLimitsCancel()
        {
            var observer = CreateObserver();
            var app = CreateAppDailyLimit(100);
            var appInfo = AppInfo.Create(app.Name);
            app.AppInfo = appInfo;

            dataService.Setup(d => d.GetFiltered<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>(),
                It.IsAny<Expression<Func<Aplication, object>>>()))
                .Returns(new List<Aplication> { app });
            dataService.Setup(d => d.GetFilteredAsync<Aplication>(It.IsAny<Expression<Func<Aplication, bool>>>())).ReturnsAsync(new List<Aplication>() { app });
            appDurationCalc.Setup(t => t.GetDuration(app, LimitSpan.Day)).ReturnsAsync(0);

            var secondAppInfo = AppInfo.Create("some app");

            observer.Initialize(new Setting() { TrackingEnabled = true });
            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(appInfo, "")));

            await Task.Delay(50);

            windowChangedNotifier.Raise(w => w.AppChanged += null, new AppChangedArgs(LogInfo.Create(secondAppInfo, "")));

            limitHandler.Verify(h => h.Handle(It.IsAny<AppLimit>()), Times.Never);
        }

        private LimitObserver CreateObserver()
        {
            return new LimitObserver(trackingService.Object,
                                     dataService.Object,
                                     windowChangedNotifier.Object,
                                     midnightNotifier.Object,
                                     limitHandler.Object,
                                     appDurationCalc.Object,
                                     mediator,
                                     syncContext);
        }

        private Aplication CreateAppDailyLimit(int limitAmount)
        {
            var app = new Aplication();
            app.ApplicationID = 7;
            app.Name = "some app";
            app.WinName = "some name";

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

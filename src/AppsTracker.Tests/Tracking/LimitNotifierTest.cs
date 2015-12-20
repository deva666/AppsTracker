using System;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Tests.Fakes;
using AppsTracker.Tracking.Limits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class LimitNotifierTest : TestMockBase
    {
        [TestMethod]
        public async Task TestLimitEvent()
        {
            var notifier = new LimitNotifier(syncContext, LimitSpan.Day);
            var limit = new AppLimit();
            AppLimit limitInEvent = null;
            var eventFired = false;

            notifier.Limit = limit;
            notifier.LimitReached += (s, e) =>
            {
                Volatile.Write(ref eventFired, true);
                Volatile.Write(ref limitInEvent, e.Limit);
            };
            notifier.Setup(TimeSpan.FromMilliseconds(100));

            await Task.Delay(150);

            Assert.IsTrue(Volatile.Read(ref eventFired));
            Assert.AreSame(Volatile.Read(ref limitInEvent), limit);
        }
    }
}

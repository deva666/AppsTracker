using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tracking.Hooks.Tests
{
    [TestClass()]
    public class AppChangedNotifierTests
    {
        [TestMethod()]
        public void WinChangedEventTest()
        {
            var winChanged = new Mock<IWinChanged>();
            var titleChanged = new Mock<ITitleChanged>();
            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object);
            var eventRaised = false;

            notifier.AppChanged += (s, e) => eventRaised = true;

            winChanged.Raise(w => w.ActiveWindowChanged += null,
                new WinChangedArgs("title", (IntPtr)10));
            Assert.IsTrue(eventRaised, "app changed event should be raised");
        }

        [TestMethod()]
        public void WinChangedEventNotRaisedTest()
        {
            var winChanged = new Mock<IWinChanged>();
            var titleChanged = new Mock<ITitleChanged>();
            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object);
            var eventRaised = false;

            notifier.AppChanged += (s, e) => eventRaised = true;

            winChanged.Raise(w => w.ActiveWindowChanged += null,
                new WinChangedArgs("title", default(IntPtr)));
            Assert.IsFalse(eventRaised, "app changed event should not be raised if event window handle is same as active handle");
        }

        [TestMethod()]
        public void TitleChangedEventNotRaisedTest()
        {
            var winChanged = new Mock<IWinChanged>();
            var titleChanged = new Mock<ITitleChanged>();
            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object);
            var eventRaised = false;

            notifier.AppChanged += (s, e) => eventRaised = true;

            titleChanged.Raise(t => t.TitleChanged += null,
                new WinChangedArgs("title", (IntPtr)10));
            Assert.IsFalse(eventRaised, "app changed event should not be raised if event window handle is not same as active handle");
        }

        [TestMethod()]
        public void TitleChangedEventRaisedTest()
        {
            var winChanged = new Mock<IWinChanged>();
            var titleChanged = new Mock<ITitleChanged>();
            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object);
            var eventRaised = false;

            winChanged.Raise(w => w.ActiveWindowChanged += null,
                new WinChangedArgs("title", (IntPtr)10));

            notifier.AppChanged += (s, e) => eventRaised = true;

            titleChanged.Raise(t => t.TitleChanged += null,
                new WinChangedArgs("other title", (IntPtr)10));
            Assert.IsTrue(eventRaised, "app changed event should be raised if event window handle issame as active handle and window titles are different");
        }
    }
}

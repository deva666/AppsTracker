using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Tests.Fakes;
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
            var winChanged = new Mock<IWindowChangedNotifier>();
            var titleChanged = new Mock<ITitleChangedNotifier>();
            var syncContext = new SyncContextMock();

            var subject = new ReplaySubject<WinChangedArgs>();

            winChanged.Setup(w => w.WinChangedObservable).Returns(subject.AsObservable());
            titleChanged.Setup(t => t.TitleChangedObservable).Returns(Observable.Return(new WinChangedArgs("title2", IntPtr.Zero)));

            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object, syncContext);
            var eventCalled = false;
            notifier.AppChangedObservable.Subscribe(w =>
            {
                Assert.AreEqual("title", w.LogInfo.WindowTitle);
                eventCalled = true;
            });
            subject.OnNext(new WinChangedArgs("title", (IntPtr)10));
            Assert.IsTrue(eventCalled);
        }

        [TestMethod()]
        public void TitleChangedEventNotRaisedTest()
        {
            var winChanged = new Mock<IWindowChangedNotifier>();
            var titleChanged = new Mock<ITitleChangedNotifier>();
            var syncContext = new SyncContextMock();
            var subject = new ReplaySubject<WinChangedArgs>();

            winChanged.Setup(w => w.WinChangedObservable).Returns(Observable.Return(new WinChangedArgs("title2", IntPtr.Zero)));
            titleChanged.Setup(t => t.TitleChangedObservable).Returns(subject.AsObservable());

            var notifier = new AppChangedNotifier(winChanged.Object, titleChanged.Object, syncContext);
            var eventCalled = false;
            notifier.AppChangedObservable.Subscribe(w =>
            {
                eventCalled = true;
            });
            subject.OnNext(new WinChangedArgs("title", (IntPtr)10));
            Assert.IsFalse(eventCalled, "app changed event should not be raised if event window handle is not same as active handle");
        }

        [TestMethod()]
        public void TitleChangedEventRaisedTest()
        {
            var windowChanged = new Mock<IWindowChangedNotifier>();
            var titleChanged = new Mock<ITitleChangedNotifier>();
            var syncContext = new SyncContextMock();

            var windowSubject = new ReplaySubject<WinChangedArgs>();
            var titleSubject = new ReplaySubject<WinChangedArgs>();

            windowChanged.Setup(w => w.WinChangedObservable).Returns(windowSubject.AsObservable());
            titleChanged.Setup(t => t.TitleChangedObservable).Returns(titleSubject.AsObservable());
            var notifier = new AppChangedNotifier(windowChanged.Object, titleChanged.Object, syncContext);

            windowSubject.OnNext(new WinChangedArgs("title", (IntPtr)10));

            var eventRaised = false;

            notifier.AppChangedObservable.Subscribe(a => { eventRaised = true; });

            titleSubject.OnNext(new WinChangedArgs("other title", (IntPtr)10));

            Assert.IsTrue(eventRaised, "app changed event should be raised if event window handle is the same as active handle and window titles are different");
        }
    }
}

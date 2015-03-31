using System;
using System.Threading;
using AppsTracker.ServiceLocation;
using AppsTracker.Tests.Fakes.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.MVVM
{
    [TestClass]
    public class AsyncPropertyTest
    {
        private volatile bool _propertyChanged = false;
        private IWorker _worker;
        private AsyncProperty<int> _prop;


        [TestInitialize]
        public void Init()
        {
            _worker = new ViewModelMock();
            _prop = new AsyncProperty<int>(FakeGet, _worker);
            _prop.PropertyChanged += prop_PropertyChanged;
        }

        void prop_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
                _propertyChanged = true;
        }

        [TestMethod]
        public void TestAsyncGet()
        {
            _propertyChanged = false;
            var result = _prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (_propertyChanged == false) { }
            result = _prop.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestAsyncReset()
        {
            _propertyChanged = false;
            _prop.Reset();
            var result = _prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
        }

        [TestMethod]
        public void TestAsyncReload()
        {
            _propertyChanged = false;
            _prop.Reload();
            var result = _prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (_propertyChanged == false) { }
            result = _prop.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestVMHostWorker()
        {
            _propertyChanged = false;
            Assert.IsFalse(_worker.Working, "Host VM should not be working");
            _prop.Reload();
            Assert.IsTrue(_worker.Working, "Host VM should be working");
            while (_propertyChanged == false) { }
            Assert.IsFalse(_worker.Working, "Host VM should not be working");
        }

        private int FakeGet()
        {
            Thread.Sleep(1100);
            return 1;
        }
    }
}

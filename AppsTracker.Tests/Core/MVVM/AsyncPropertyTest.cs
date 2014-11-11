using System;
using System.Threading;
using AppsTracker.MVVM;
using AppsTracker.Tests.Fakes.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.MVVM
{
    [TestClass]
    public class AsyncPropertyTest
    {
        private volatile bool _propertyChanged = false;
        private ViewModelMock vm;
        private AsyncProperty<int> prop;


        [TestInitialize]
        public void Init()
        {
            vm = new ViewModelMock();
            prop = new AsyncProperty<int>(FakeGet, vm);
            prop.PropertyChanged += prop_PropertyChanged;
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
            var result = prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (_propertyChanged == false) { }
            result = prop.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestAsyncReset()
        {
            _propertyChanged = false;
            prop.Reset();
            var result = prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (_propertyChanged == false) { }
            result = prop.Result;
            Assert.IsTrue(result == default(int), "Result value mismatch");
        }

        [TestMethod]
        public void TestAsyncReload()
        {
            _propertyChanged = false;
            prop.Reload();
            var result = prop.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (_propertyChanged == false) { }
            result = prop.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestVMHostWorker()
        {
            _propertyChanged = false;
            Assert.IsFalse(vm.Working, "Host VM should not be working");
            prop.Reload();
            Assert.IsTrue(vm.Working, "Host VM should be working");
            while (_propertyChanged == false) { }
            Assert.IsFalse(vm.Working, "Host VM should not be working");
        }

        private int FakeGet()
        {
            Thread.Sleep(1100);
            return 1;
        }
    }
}

using System.Threading;
using AppsTracker.MVVM;
using AppsTracker.Tests.Fakes.MVVM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.MVVM
{
    [TestClass]
    public class AsyncPropertyTest
    {
        private volatile bool propertyChanged = false;
        private IWorker worker;
        private AsyncProperty<int> asyncProperty;


        [TestInitialize]
        public void Init()
        {
            worker = new ViewModelMock();
            asyncProperty = new TaskRunner<int>(FakeGet, worker);
            asyncProperty.PropertyChanged += OnPropertyChanged;
        }

        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Result")
                propertyChanged = true;
        }

        [TestMethod]
        public void TestAsyncGet()
        {
            propertyChanged = false;
            var result = asyncProperty.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (propertyChanged == false) { }
            result = asyncProperty.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestAsyncReset()
        {
            propertyChanged = false;
            asyncProperty.Reset();
            var result = asyncProperty.Result;
            Assert.IsTrue(result == default(int), "Default result value");
        }

        [TestMethod]
        public void TestAsyncReload()
        {
            propertyChanged = false;
            asyncProperty.Reload();
            var result = asyncProperty.Result;
            Assert.IsTrue(result == default(int), "Default result value");
            while (propertyChanged == false) { }
            result = asyncProperty.Result;
            Assert.IsTrue(result == 1, "Result value mismatch");
        }

        [TestMethod]
        public void TestVMHostWorker()
        {
            propertyChanged = false;
            Assert.IsFalse(worker.Working, "Host VM should not be working");
            asyncProperty.Reload();
            Assert.IsTrue(worker.Working, "Host VM should be working");
            while (propertyChanged == false) { }
            Assert.IsFalse(worker.Working, "Host VM should not be working");
        }

        private int FakeGet()
        {
            Thread.Sleep(200);
            return 1;
        }
    }
}

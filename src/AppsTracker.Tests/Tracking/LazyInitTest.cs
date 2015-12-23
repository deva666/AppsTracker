using System;
using AppsTracker.Tracking;
using AppsTracker.Tracking.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Tracking
{
    [TestClass]
    public class LazyInitTest
    {
        [TestMethod]
        public void TestLazyInitialization()
        {
            var testObject = new TestObject();
            var lazy = new LazyInit<TestObject>(() => testObject);

            Assert.AreSame(testObject, lazy.Component);
        }

        [TestMethod]
        public void TestLazyOnInitCall()
        {
            var testObject = new TestObject();
            var lazy = new LazyInit<TestObject>(()=> testObject, t => t.OnInitFired = true);

            lazy.Enabled = true;

            Assert.IsTrue(testObject.OnInitFired);
            Assert.IsFalse(testObject.OnDisposeFired);
        }

        [TestMethod]
        public void TestLazyOnDisposeCall()
        {
            var testObject = new TestObject();
            var lazy = new LazyInit<TestObject>(() => testObject, t => t.OnInitFired = true, t => t.OnDisposeFired = true);

            lazy.Enabled = true;

            Assert.IsTrue(testObject.OnInitFired);
            Assert.IsFalse(testObject.OnDisposeFired);

            lazy.Enabled = false;

            Assert.IsTrue(testObject.OnDisposeFired);
        }

        private class TestObject : IDisposable
        {
            public bool OnInitFired { get; set; }

            public bool OnDisposeFired { get; set; }

            public void Dispose()
            {    
            }
        }
    }
}

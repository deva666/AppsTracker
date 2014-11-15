using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using AppsTracker.Common.Utils;

namespace AppsTracker.Tests.Common
{
    [TestClass]
    public class EnsureTest
    {
        [TestMethod]
        public void TestCustomException()
        {
            try
            {
                Ensure.Condition<InvalidOperationException>(() => false, "Test");
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException), "Exception type don't match");
                Assert.IsTrue(ex.Message == "Test", "Exception message don't match");
            }
        }

        [TestMethod]
        public void TestCustomException2()
        {
            try
            {
                Ensure.Condition<InvalidOperationException>(false, "Test");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException), "Exception type don't match");
                Assert.IsTrue(ex.Message == "Test", "Exception message don't match");
            }
        }

        [TestMethod]
        public void TestNullArgumentException()
        {
            try
            {
                Ensure.NotNull(null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentNullException), "Exception type don't match");
            }
        }
    }
}

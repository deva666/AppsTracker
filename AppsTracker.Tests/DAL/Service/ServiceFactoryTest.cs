#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion


using System;

using AppsTracker.Data.Service;
using AppsTracker.Tests.Fakes.Service;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.DAL.Service
{
    [TestClass]
    public class ServiceFactoryTest
    {
        [TestInitialize]
        public void Init()
        {
            if (ServiceFactory.ContainsKey<IDataService>() == false)
                ServiceFactory.Register<IDataService>(() => new AppsServiceMock());
            if (ServiceFactory.ContainsKey<IChartService>() == false)
                ServiceFactory.Register<IChartService>(() => new ChartServiceMock());
        }
        [TestMethod]
        public void TestRegisterMethod()
        {
            Assert.IsInstanceOfType(ServiceFactory.Get<IDataService>(), typeof(AppsServiceMock), "IAppsService type don't match");
            Assert.IsInstanceOfType(ServiceFactory.Get<IChartService>(), typeof(ChartServiceMock), "IChartService type don't match");
        }

        [TestMethod]
        public void TestDoubleRegistration()
        {
            try
            {
                ServiceFactory.Register<IDataService>(() => new AppsServiceMock());
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(InvalidOperationException), "Exception types don't match");
                Assert.IsInstanceOfType(ServiceFactory.Get<IDataService>(), typeof(AppsServiceMock), "Failed get after double registration");
            }
        }
    }
}

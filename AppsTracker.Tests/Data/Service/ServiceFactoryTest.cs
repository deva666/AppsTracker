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
    public class ServiceFactoryTest : TestBase
    {
        [TestInitialize]
        public void Init()
        {
            base.Initialize();           
        }
        [TestMethod]
        public void TestRegisterMethod()
        {
            Assert.IsInstanceOfType(ServiceFactory.Get<IDataService>(), typeof(AppsServiceMock), "IAppsService type don't match");
            Assert.IsInstanceOfType(ServiceFactory.Get<IChartService>(), typeof(ChartServiceMock), "IChartService type don't match");
        }      
    }
}

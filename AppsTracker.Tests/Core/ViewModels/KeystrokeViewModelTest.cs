using System;
using System.Collections.Generic;

using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.Pages.ViewModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class KeystrokeViewModelTest
    {
        [TestInitialize]
        public void Init()
        {
            if (!ServiceFactory.ContainsKey<IAppsService>())
                ServiceFactory.Register<IAppsService>(() => new AppsServiceMock());
        }

        [TestMethod]
        public void TestLoader()
        {
            var vm = new Data_keystrokesViewModel();
            vm.LoadContent();

            Assert.IsTrue(vm.IsContentLoaded, "Content not loaded");
            Assert.IsNotNull(vm.LogList, "LogList not loaded");
            Assert.IsInstanceOfType(vm.LogList, typeof(IEnumerable<Log>), "Log types don't match");
        }
    }
}

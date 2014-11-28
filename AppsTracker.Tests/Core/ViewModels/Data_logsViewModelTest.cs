using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Pages.ViewModels;
using AppsTracker.DAL.Service;
using AppsTracker.Tests.Fakes.Service;
using AppsTracker.Models.EntityModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class Data_logsViewModelTest
    {
        [TestInitialize]
        public void Init()
        {
            if (ServiceFactory.ContainsKey<IAppsService>() == false)
                ServiceFactory.Register<IAppsService>(() => new AppsServiceMock());
            if (ServiceFactory.ContainsKey<IChartService>() == false)
                ServiceFactory.Register<IChartService>(() => new ChartServiceMock());
        }

        [TestMethod]
        public void TestLoaders()
        {
            var vm = new Data_logsViewModel();

            var apps = vm.AplicationList.Result;
            var topApps = vm.TopAppsList.Result;
            var topWindows = vm.TopWindowsList.Result;
            var chartList = vm.ChartList.Result;

            while (true)
            {
                if (vm.AplicationList.Task.Status == TaskStatus.RanToCompletion)
                    break;
            }


            Assert.IsNotNull(vm.AplicationList.Result, "App list not loaded");
            //Assert.IsInstanceOfType(vm.AplicationList.Result, typeof(List<Aplication>), "App list type mismatch");
        }
    }
}

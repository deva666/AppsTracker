using System.Threading.Tasks;
using AppsTracker.Data.Service;
using AppsTracker.Pages.ViewModels;
using AppsTracker.Tests.Fakes.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class Data_logsViewModelTest
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

            System.Threading.Thread.Sleep(100);

            Assert.IsNotNull(vm.AplicationList.Result, "App list not loaded");
        }
    }
}

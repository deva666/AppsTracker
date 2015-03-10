using System.Threading.Tasks;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class Data_logsViewModelTest : TestBase
    {
        [TestInitialize]
        public void Init()
        {
            base.Initialize();
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

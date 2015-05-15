using System.Threading.Tasks;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class AppDetailsViewModelTest : TestMockBase
    {
        [TestInitialize]
        public void Init()
        {

        }

        [TestMethod]
        public void TestLoaders()
        {
        }
        //{
        //    var vm = new AppDetailsViewModel();

        //    var apps = vm.AppList.Result;
        //    var topApps = vm.AppSummaryList.Result;
        //    var topWindows = vm.WindowSummaryList.Result;
        //    var chartList = vm.WindowDurationList.Result;

        //    while (true)
        //    {
        //        if (vm.AppList.Task.Status == TaskStatus.RanToCompletion)
        //            break;
        //    }

        //    System.Threading.Thread.Sleep(100);

        //    Assert.IsNotNull(vm.AppList.Result, "App list not loaded");
        //}
    }
}

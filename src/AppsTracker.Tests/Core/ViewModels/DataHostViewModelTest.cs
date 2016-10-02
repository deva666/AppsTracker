using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass]
    public class DataHostViewModelTest : TestMockBase
    {
        private DataHostViewModel CreateViewModel()
        {
            var viewModel = new DataHostViewModel(GetAppDetailsVMFactory(),
                                                  GetScreenshotsVMFactory(),
                                                  GetDaySummaryVMFactory());
            return viewModel;
        }

        [TestMethod]
        public void TestNavigateToAppDetails()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToAppDetailsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(AppDetailsViewModel));
        }

        [TestMethod]
        public void TestNavigateToDaySummary()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToDaySummaryCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(DaySummaryViewModel));
        }

        [TestMethod]
        public void TestNavigateToScreenshots()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToScreenshotsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(ScreenshotsViewModel));
        }
    }
}

using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass()]
    public class StatisticsHostViewModelTests : TestMockBase
    {
        private StatisticsHostViewModel CreateViewModel()
        {
            return new StatisticsHostViewModel(GetUserStatsVMFactory(),
                                               GetAppStatsVMFactory(),
                                               GetDailyAppUsageVMFactory(),
                                               GetScreenshotStatsVMFactory(),
                                               GetCategoryStatsVMFactory());
        }

        [TestMethod()]
        public void TestNavigateToUserStats()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToUserStatsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(UserStatsViewModel));
        }

        [TestMethod()]
        public void TestNavigateToAppStats()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToAppStatsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(AppStatsViewModel));
        }

        [TestMethod()]
        public void TestNavigateToDailyAppUsage()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToDailyAppUsageCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(DailyAppUsageViewModel));
        }

        [TestMethod()]
        public void TestNavigateToScreenshotsStats()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToScreenshotsStatsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(ScreenshotsStatsViewModel));
        }

        [TestMethod()]
        public void TestNavigateToCategoruyStats()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToCategoryStatsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(CategoryStatsViewModel));
        }
    }
}

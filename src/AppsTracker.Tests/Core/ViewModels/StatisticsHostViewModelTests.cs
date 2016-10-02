using System.Collections.Generic;
using AppsTracker.Domain.Apps;
using AppsTracker.Domain.Screenshots;
using AppsTracker.Domain.Usages;
using AppsTracker.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AppsTracker.Tests.Core.ViewModels
{
    [TestClass()]
    public class StatisticsHostViewModelTests : TestMockBase
    {
        [TestInitialize]
        public void Setup()
        {
            usageOverViewUseCase.Setup(u => u.Get(It.IsAny<string>())).Returns(new List<UsageOverview>());
            appDurationUseCase.Setup(u => u.Get()).Returns(new List<AppDuration>());
            dailyAppDurationUseCase.Setup(u => u.Get(It.IsAny<string>())).Returns(new List<DailyAppDuration>());
            screenshotModelUseCase.Setup(u => u.Get()).Returns(new List<ScreenshotOverview>());
            dailyScreenshotModelUseCase.Setup(u => u.Get(It.IsAny<string>())).Returns(new List<DailyScreenshotModel>());
            categoryDurationUseCase.Setup(u => u.Get()).Returns(new List<CategoryDuration>());
            dailyCategoryDurationUseCase.Setup(u => u.Get(It.IsAny<string>())).Returns(new List<DailyCategoryDuration>());
        }

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
        public void TestNavigateToAppStats()        {
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
        public void TestNavigateToCategoryStats()
        {
            var viewModel = CreateViewModel();
            viewModel.GoToCategoryStatsCommand.Execute(null);
            Assert.IsInstanceOfType(viewModel.SelectedChild, typeof(CategoryStatsViewModel));
        }
    }
}

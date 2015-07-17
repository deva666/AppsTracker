using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.ViewModels;
using AppsTracker.Common.Communication;
using Moq;
using AppsTracker.Tracking.Hooks;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Tests.Fakes;
using AppsTracker.Tracking;

namespace AppsTracker.Tests
{
    public abstract class TestMockBase
    {
        protected Mock<IDataService> dataService = new Mock<IDataService>();
        protected Mock<ITrackingService> trackingService = new Mock<ITrackingService>();
        protected Mock<ISqlSettingsService> settingsService = new Mock<ISqlSettingsService>();
        protected Mock<IXmlSettingsService> xmlSettingsService = new Mock<IXmlSettingsService>();
        protected Mock<IStatsService> statsService = new Mock<IStatsService>();
        protected Mock<ICategoriesService> categoriesService = new Mock<ICategoriesService>();
        protected Mock<IWindowService> windowService = new Mock<IWindowService>();
        protected Mock<IAppChangedNotifier> windowChangedNotifier = new Mock<IAppChangedNotifier>();
        protected Mock<ILimitHandler> limitHandler = new Mock<ILimitHandler>();
        protected Mock<IMidnightNotifier> midnightNotifier = new Mock<IMidnightNotifier>();
        protected Mock<IScreenshotFactory> screenshotFactory = new Mock<IScreenshotFactory>();
        protected readonly Mock<IAppChangedNotifier> appChangedNotifier = new Mock<IAppChangedNotifier>();
        protected readonly Mock<IScreenshotTracker> screenshotTracker = new Mock<IScreenshotTracker>();

        protected readonly IMediator mediator = new Mediator();
        protected readonly ISyncContext syncContext = new SyncContextMock();

        protected ExportFactory<AppDetailsViewModel> GetAppDetailsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<AppDetailsViewModel, Action>>(
                    () => new Tuple<AppDetailsViewModel, Action>(
                        new AppDetailsViewModel(dataService.Object,
                            statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));
            return new ExportFactory<AppDetailsViewModel>(tupleFactory);
        }

        protected ExportFactory<ScreenshotsViewModel> GetScreenshotsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<ScreenshotsViewModel, Action>>(
                    () => new Tuple<ScreenshotsViewModel, Action>(
                        new ScreenshotsViewModel(dataService.Object,
                            settingsService.Object,
                            trackingService.Object,
                            windowService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<ScreenshotsViewModel>(tupleFactory);
        }

        protected ExportFactory<DaySummaryViewModel> GetDaySummaryVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<DaySummaryViewModel, Action>>(
                    () => new Tuple<DaySummaryViewModel, Action>(
                        new DaySummaryViewModel(dataService.Object,
                            statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<DaySummaryViewModel>(tupleFactory);
        }

        protected ExportFactory<DataHostViewModel> GetDataHostVMFactory()
        {
            var appDetailsVMFactory = GetAppDetailsVMFactory();
            var screenshotsVMFactory = GetScreenshotsVMFactory();
            var daySummaryVMFactory = GetDaySummaryVMFactory();
            var tupleFactory =
                new Func<Tuple<DataHostViewModel, Action>>(
                () => new Tuple<DataHostViewModel, Action>(
                        new DataHostViewModel(appDetailsVMFactory,
                            screenshotsVMFactory,
                            daySummaryVMFactory),
                            ExportFactoryContextRelease));

            return new ExportFactory<DataHostViewModel>(tupleFactory);
        }

        protected ExportFactory<UserStatsViewModel> GetUserStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<UserStatsViewModel, Action>>(
                    () => new Tuple<UserStatsViewModel, Action>(
                        new UserStatsViewModel(statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<UserStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<AppStatsViewModel> GetAppStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<AppStatsViewModel, Action>>(
                    () => new Tuple<AppStatsViewModel, Action>(
                        new AppStatsViewModel(statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<AppStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<DailyAppUsageViewModel> GetDailyAppUsageVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<DailyAppUsageViewModel, Action>>(
                    () => new Tuple<DailyAppUsageViewModel, Action>(
                        new DailyAppUsageViewModel(statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<DailyAppUsageViewModel>(tupleFactory);
        }


        protected ExportFactory<ScreenshotsStatsViewModel> GetScreenshotStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<ScreenshotsStatsViewModel, Action>>(
                    () => new Tuple<ScreenshotsStatsViewModel, Action>(
                        new ScreenshotsStatsViewModel(statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<ScreenshotsStatsViewModel>(tupleFactory);
        }

        protected ExportFactory<CategoryStatsViewModel> GetCategoryStatsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<CategoryStatsViewModel, Action>>(
                    () => new Tuple<CategoryStatsViewModel, Action>(
                        new CategoryStatsViewModel(statsService.Object,
                            trackingService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<CategoryStatsViewModel>(tupleFactory);
        }


        protected ExportFactory<StatisticsHostViewModel> GetStatisticsHostVMFactory()
        {
            var userStatsVMFactory = GetUserStatsVMFactory();
            var appStatsVMFactory = GetAppStatsVMFactory();
            var dailyAppUsageVMFactory = GetDailyAppUsageVMFactory();
            var screenshotStatsVMFactory = GetScreenshotStatsVMFactory();
            var categoryStatsVMFactory = GetCategoryStatsVMFactory();
            var tupleFactory =
                new Func<Tuple<StatisticsHostViewModel, Action>>(
                    () => new Tuple<StatisticsHostViewModel, Action>(
                        new StatisticsHostViewModel(userStatsVMFactory,
                            appStatsVMFactory,
                            dailyAppUsageVMFactory,
                            screenshotStatsVMFactory,
                            categoryStatsVMFactory),
                            ExportFactoryContextRelease));

            return new ExportFactory<StatisticsHostViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsGeneralViewModel> GetSettingsGeneralVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsGeneralViewModel, Action>>(
                    () => new Tuple<SettingsGeneralViewModel, Action>(
                        new SettingsGeneralViewModel(windowService.Object,
                            settingsService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsGeneralViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsTrackingViewModel> GetSettingsLoggingVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsTrackingViewModel, Action>>(
                    () => new Tuple<SettingsTrackingViewModel, Action>(
                        new SettingsTrackingViewModel(settingsService.Object, mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsTrackingViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsScreenshotsViewModel> GetSettingsScreenshotsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsScreenshotsViewModel, Action>>(
                    () => new Tuple<SettingsScreenshotsViewModel, Action>(
                        new SettingsScreenshotsViewModel(settingsService.Object,
                            trackingService.Object,
                            dataService.Object,
                            windowService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsScreenshotsViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsPasswordViewModel> GetSettingsPasswordVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsPasswordViewModel, Action>>(
                    () => new Tuple<SettingsPasswordViewModel, Action>(
                        new SettingsPasswordViewModel(windowService.Object,
                            settingsService.Object,
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsPasswordViewModel>(tupleFactory);
        }


        protected ExportFactory<ICategoriesService> GetCategoriesServiceFactory()
        {
            var tupleFactory = new Func<Tuple<ICategoriesService, Action>>(
                () => new Tuple<ICategoriesService, Action>(
                    categoriesService.Object, ExportFactoryContextRelease));

            return new ExportFactory<ICategoriesService>(tupleFactory);
        }

        protected ExportFactory<SettingsAppCategoriesViewModel> GetSettingsAppCategoriesVMFactory()
        {
            var categoriesFactory = GetCategoriesServiceFactory();
            var tupleFactory =
                new Func<Tuple<SettingsAppCategoriesViewModel, Action>>(
                    () => new Tuple<SettingsAppCategoriesViewModel, Action>(
                        new SettingsAppCategoriesViewModel(categoriesFactory,
                            mediator,
                            trackingService.Object),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsAppCategoriesViewModel>(tupleFactory);
        }


        protected ExportFactory<SettingsLimitsViewModel> GetSettingsLimitsVMFactory()
        {
            var tupleFactory =
                new Func<Tuple<SettingsLimitsViewModel, Action>>(
                    () => new Tuple<SettingsLimitsViewModel, Action>(
                        new SettingsLimitsViewModel(dataService.Object,
                            trackingService.Object, 
                            mediator),
                            ExportFactoryContextRelease));

            return new ExportFactory<SettingsLimitsViewModel>(tupleFactory);
        }

        protected ExportFactory<SettingsHostViewModel> GetSettingsHostVMFactory()
        {
            var settingsGeneralVMFactory = GetSettingsGeneralVMFactory();
            var settingsLoggingVMFactory = GetSettingsLoggingVMFactory();
            var settingsScreenshotsVMFactory = GetSettingsScreenshotsVMFactory();
            var settingsPasswordVMFactory = GetSettingsPasswordVMFactory();
            var settingsCategoriesVMFactory = GetSettingsAppCategoriesVMFactory();
            var settingsLimitsVMFactory = GetSettingsLimitsVMFactory();

            var tupleFactory = new Func<Tuple<SettingsHostViewModel, Action>>(
                () => new Tuple<SettingsHostViewModel, Action>(
                    new SettingsHostViewModel(settingsGeneralVMFactory,
                        settingsLoggingVMFactory,
                        settingsScreenshotsVMFactory,
                        settingsPasswordVMFactory,
                        settingsCategoriesVMFactory,
                        settingsLimitsVMFactory),
                        ExportFactoryContextRelease));
            return new ExportFactory<SettingsHostViewModel>(tupleFactory);
        }

        protected void ExportFactoryContextRelease()
        {

        }
    }
}

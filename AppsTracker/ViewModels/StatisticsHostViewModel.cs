#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class StatisticsHostViewModel : HostViewModel
    {
        public override string Title { get { return "statistics"; } }


        private ICommand goToUserStatsCommand;

        public ICommand GoToUserStatsCommand
        {
            get { return goToUserStatsCommand ?? (goToUserStatsCommand = new DelegateCommand(GoToUserStats)); }
        }


        private ICommand goToAppStatsCommand;

        public ICommand GoToAppStatsCommand
        {
            get { return goToAppStatsCommand ?? (goToAppStatsCommand = new DelegateCommand(GoToAppStats)); }
        }


        private ICommand goToDailyAppUsageCommand;

        public ICommand GoToDailyAppUsageCommand
        {
            get { return goToDailyAppUsageCommand ?? (goToDailyAppUsageCommand = new DelegateCommand(GoToDailyAppUsage)); }
        }


        private ICommand goToScreenshotsStatsCommand;

        public ICommand GoToScreenshotsStatsCommand
        {
            get { return goToScreenshotsStatsCommand ?? (goToScreenshotsStatsCommand = new DelegateCommand(GoToScreenshotStats)); }
        }


        private ICommand goToCategoryStatsCommand;

        public ICommand GoToCategoryStatsCommand
        {
            get { return goToCategoryStatsCommand ?? (goToCategoryStatsCommand = new DelegateCommand(GoToCategoryStats)); }
        }

        [ImportingConstructor]
        public StatisticsHostViewModel(ExportFactory<UserStatsViewModel> userStatsVMFactory,
                                       ExportFactory<AppStatsViewModel> appStatsVMFactory,
                                       ExportFactory<DailyAppUsageViewModel> dailyAppUsageVMFactory,
                                       ExportFactory<ScreenshotsStatsViewModel> screenshotStatsVMFactory,
                                       ExportFactory<CategoryStatsViewModel> categoryStatsVMFactory)
        {
            RegisterChild(() => ProduceValue(userStatsVMFactory));
            RegisterChild(() => ProduceValue(appStatsVMFactory));
            RegisterChild(() => ProduceValue(dailyAppUsageVMFactory));
            RegisterChild(() => ProduceValue(screenshotStatsVMFactory));
            RegisterChild(() => ProduceValue(categoryStatsVMFactory));
            
            SelectedChild = GetChild(typeof(UserStatsViewModel));
        }


        private void GoToUserStats()
        {
            SelectedChild = GetChild<UserStatsViewModel>();
        }


        private void GoToAppStats()
        {
            SelectedChild = GetChild<AppStatsViewModel>();
        }


        private void GoToDailyAppUsage()
        {
            SelectedChild = GetChild<DailyAppUsageViewModel>();
        }


        private void GoToScreenshotStats()
        {
            SelectedChild = GetChild<ScreenshotsStatsViewModel>();
        }


        private void GoToCategoryStats()
        {
            SelectedChild = GetChild<CategoryStatsViewModel>();
        }
    }
}

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.MVVM;
using System.Windows.Input;

namespace AppsTracker.ViewModels
{
    internal sealed class StatisticsHostViewModel : HostViewModel
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

        public StatisticsHostViewModel()
        {
            RegisterChild(() => new UserStatsViewModel());
            RegisterChild(() => new AppStatsViewModel());
            RegisterChild(() => new DailyAppUsageViewModel());
            RegisterChild(() => new ScreenshotsStatsViewModel());
            RegisterChild(() => new CategoryStatsViewModel());

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

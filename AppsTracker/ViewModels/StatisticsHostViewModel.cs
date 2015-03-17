#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class StatisticsHostViewModel : HostViewModel
    {
        public override string Title { get { return "statistics"; } }

        public StatisticsHostViewModel()
        {
            RegisterChild<Statistics_usersViewModel>(() => new Statistics_usersViewModel());
            RegisterChild<AppStatsViewModel>(() => new AppStatsViewModel());
            RegisterChild<Statistics_dailyAppUsageViewModel>(() => new Statistics_dailyAppUsageViewModel());
            RegisterChild<Statistics_keystrokesViewModel>(() => new Statistics_keystrokesViewModel());
            RegisterChild<Statistics_screenshotsViewModel>(() => new Statistics_screenshotsViewModel());

            SelectedChild = GetChild(typeof(Statistics_usersViewModel));
        }
    }
}

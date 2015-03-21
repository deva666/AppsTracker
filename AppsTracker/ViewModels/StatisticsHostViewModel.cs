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
            RegisterChild(() => new UserStatsViewModel());
            RegisterChild(() => new AppStatsViewModel());
            RegisterChild(() => new Statistics_dailyAppUsageViewModel());
            RegisterChild(() => new Statistics_screenshotsViewModel());

            SelectedChild = GetChild(typeof(UserStatsViewModel));
        }
    }
}

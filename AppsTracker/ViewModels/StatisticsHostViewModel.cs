#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.MVVM;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.ViewModels
{
    internal sealed class StatisticsHostViewModel : HostViewModel
    {
        public override string Title { get { return "statistics"; } }

        public StatisticsHostViewModel()
        {
            Register<Statistics_usersViewModel>(() => new Statistics_usersViewModel());
            Register<Statistics_appUsageViewModel>(() => new Statistics_appUsageViewModel());
            Register<Statistics_dailyAppUsageViewModel>(() => new Statistics_dailyAppUsageViewModel());
            Register<Statistics_keystrokesViewModel>(() => new Statistics_keystrokesViewModel());
            Register<Statistics_screenshotsViewModel>(() => new Statistics_screenshotsViewModel());

            SelectedChild = Resolve(typeof(Statistics_usersViewModel));
        }
    }
}

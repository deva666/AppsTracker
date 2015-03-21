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
    internal sealed class DataHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "data"; }
        }

        public DataHostViewModel()
        {
            RegisterChild(() => new AppDetailsViewModel());
            RegisterChild(() => new ScreenshotsViewModel());
            RegisterChild(() => new DaySummaryViewModel());

            SelectedChild = GetChild(typeof(AppDetailsViewModel));
        }

    }
}

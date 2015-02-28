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
    internal sealed class DataHostViewModel : HostViewModel
    {
        public override string Title
        {
            get
            {
                return "data";
            }
        }

        public DataHostViewModel()
        {
            this.RegisterChild<Data_logsViewModel>(() => new Data_logsViewModel());
            this.RegisterChild<Data_keystrokesViewModel>(() => new Data_keystrokesViewModel());
            this.RegisterChild<Data_screenshotsViewModel>(() => new Data_screenshotsViewModel());
            this.RegisterChild<Data_dayViewModel>(() => new Data_dayViewModel());

            this.SelectedChild = GetChild(typeof(Data_logsViewModel));
        }

        protected override void Disposing()
        {
            if (_selectedChild != null)
                _selectedChild.Dispose();
            _selectedChild = null;
            base.Disposing();
        }
    }
}

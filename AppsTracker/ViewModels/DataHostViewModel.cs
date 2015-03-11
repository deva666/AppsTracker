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
            this.RegisterChild<AppDetailsViewModel>(() => new AppDetailsViewModel());
            this.RegisterChild<KeystrokesViewModel>(() => new KeystrokesViewModel());
            this.RegisterChild<ScreenshotsViewModel>(() => new ScreenshotsViewModel());
            this.RegisterChild<Data_dayViewModel>(() => new Data_dayViewModel());

            this.SelectedChild = GetChild(typeof(AppDetailsViewModel));
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

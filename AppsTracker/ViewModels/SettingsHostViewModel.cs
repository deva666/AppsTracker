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
    internal sealed class SettingsHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "settings"; }
        }

        public SettingsHostViewModel()
        {
            Register<SettingsGeneralViewModel>(() => new SettingsGeneralViewModel());
            Register<SettingsLoggingViewModel>(() => new SettingsLoggingViewModel());
            Register<SettingsScreenshotsViewModel>(() => new SettingsScreenshotsViewModel());

            SelectedChild = Resolve(typeof(SettingsGeneralViewModel));
        }
    }
}

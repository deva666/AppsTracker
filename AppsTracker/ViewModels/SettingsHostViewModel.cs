#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.ServiceLocation;

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
            RegisterChild<SettingsGeneralViewModel>(() => new SettingsGeneralViewModel());
            RegisterChild<SettingsLoggingViewModel>(() => new SettingsLoggingViewModel());
            RegisterChild<SettingsScreenshotsViewModel>(() => new SettingsScreenshotsViewModel());
            RegisterChild<SettingsPasswordViewModel>(() => new SettingsPasswordViewModel());
            RegisterChild<SettingsAppCategoriesViewModel>(() => new SettingsAppCategoriesViewModel());

            SelectedChild = GetChild(typeof(SettingsGeneralViewModel));
        }
    }
}

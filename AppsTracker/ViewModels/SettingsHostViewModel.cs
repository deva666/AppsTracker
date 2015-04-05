#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Windows.Input;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    internal sealed class SettingsHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "settings"; }
        }


        private ICommand goToGeneralCommand;

        public ICommand GoToGeneralCommand
        {
            get{ return goToGeneralCommand ?? (goToGeneralCommand = new DelegateCommand(GoToGeneral)); }
        }


        private ICommand goToLoggingCommand;

        public ICommand GoToLoggingCommand
        {
            get{ return goToLoggingCommand ?? (goToLoggingCommand = new DelegateCommand(GoToLogging)); }
        }

        private ICommand goToScreenshotsCommand;

        public ICommand GoToScreenshotsCommand
        {
            get{ return goToScreenshotsCommand ?? (goToScreenshotsCommand = new DelegateCommand(GoToScreenshots)); }
        }


        private ICommand goToPasswordCommand;

        public ICommand GoToPasswordCommand
        {
            get{ return goToPasswordCommand ?? (goToPasswordCommand = new DelegateCommand(GoToPassword)); }
        }


        private ICommand goToAppCategoriesCommand;

        public ICommand GoToAppCategoriesCommand
        {
            get{ return goToAppCategoriesCommand ?? (goToAppCategoriesCommand = new DelegateCommand(GoToAppCategories)); }
        }


        public SettingsHostViewModel()
        {
            RegisterChild<SettingsGeneralViewModel>(() => new SettingsGeneralViewModel());
            RegisterChild<SettingsLoggingViewModel>(() => new SettingsLoggingViewModel());
            RegisterChild<SettingsScreenshotsViewModel>(() => new SettingsScreenshotsViewModel());
            RegisterChild<SettingsPasswordViewModel>(() => new SettingsPasswordViewModel());
            RegisterChild<SettingsAppCategoriesViewModel>(() => new SettingsAppCategoriesViewModel());

            SelectedChild = GetChild<SettingsGeneralViewModel>();
        }


        private void GoToGeneral()
        {
            SelectedChild = GetChild<SettingsGeneralViewModel>();
        }

        
        private void GoToLogging()
        {
            SelectedChild = GetChild<SettingsLoggingViewModel>();
        }

        
        private void GoToScreenshots()
        {
            SelectedChild = GetChild<SettingsScreenshotsViewModel>();
        }


        private void GoToPassword()
        {
            SelectedChild = GetChild<SettingsPasswordViewModel>();
        }


        private void GoToAppCategories()
        {
            SelectedChild = GetChild<SettingsAppCategoriesViewModel>();
        }
    }
}

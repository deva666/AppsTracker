#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class SettingsHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "settings"; }
        }


        private ICommand goToGeneralCommand;

        public ICommand GoToGeneralCommand
        {
            get { return goToGeneralCommand ?? (goToGeneralCommand = new DelegateCommand(GoToGeneral)); }
        }


        private ICommand goToLoggingCommand;

        public ICommand GoToLoggingCommand
        {
            get { return goToLoggingCommand ?? (goToLoggingCommand = new DelegateCommand(GoToLogging)); }
        }

        private ICommand goToScreenshotsCommand;

        public ICommand GoToScreenshotsCommand
        {
            get { return goToScreenshotsCommand ?? (goToScreenshotsCommand = new DelegateCommand(GoToScreenshots)); }
        }


        private ICommand goToPasswordCommand;

        public ICommand GoToPasswordCommand
        {
            get { return goToPasswordCommand ?? (goToPasswordCommand = new DelegateCommand(GoToPassword)); }
        }


        private ICommand goToAppCategoriesCommand;

        public ICommand GoToAppCategoriesCommand
        {
            get { return goToAppCategoriesCommand ?? (goToAppCategoriesCommand = new DelegateCommand(GoToAppCategories)); }
        }


        private ICommand goToAppLimitsCommand;

        public ICommand GoToAppLimitsCommand
        {
            get
            {
                return goToAppLimitsCommand ?? (goToAppLimitsCommand = new DelegateCommand(GoToAppLimits));
            }
        }


        [ImportingConstructor]
        public SettingsHostViewModel(ExportFactory<SettingsGeneralViewModel> generalVMFactory,
                                     ExportFactory<SettingsLoggingViewModel> loggingVMFactory,
                                     ExportFactory<SettingsScreenshotsViewModel> screenshotsVMFactory,
                                     ExportFactory<SettingsPasswordViewModel> passwordVMFactory,
                                     ExportFactory<SettingsAppCategoriesViewModel> appCategoriesVMFactory,
                                     ExportFactory<SettingsLimitsViewModel> appLimitsVMFactory)
        {
            RegisterChild(() => ProduceValue(generalVMFactory));
            RegisterChild(() => ProduceValue(loggingVMFactory));
            RegisterChild(() => ProduceValue(screenshotsVMFactory));
            RegisterChild(() => ProduceValue(passwordVMFactory));
            RegisterChild(() => ProduceValue(appCategoriesVMFactory));
            RegisterChild(() => ProduceValue(appLimitsVMFactory));

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


        private void GoToAppLimits()
        {
            SelectedChild = GetChild<SettingsLimitsViewModel>();
        }
    }
}

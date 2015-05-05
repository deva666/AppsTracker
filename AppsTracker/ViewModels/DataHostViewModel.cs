#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Windows.Input;
using AppsTracker.MVVM;
using System.ComponentModel.Composition;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class DataHostViewModel : HostViewModel
    {
        public override string Title
        {
            get { return "data"; }
        }


        private ICommand goToAppDetailsCommand;

        public ICommand GoToAppDetailsCommand
        {
            get { return goToAppDetailsCommand ?? (goToAppDetailsCommand = new DelegateCommand(GoToAppDetails)); }
        }


        private ICommand goToScreenshotsCommand;

        public ICommand GoToScreenshotsCommand
        {
            get { return goToScreenshotsCommand ?? (goToScreenshotsCommand = new DelegateCommand(GoToScreenshots)); }
        }


        private ICommand goToDaySummaryCommand;

        public ICommand GoToDaySummaryCommand
        {
            get { return goToDaySummaryCommand ?? (goToDaySummaryCommand = new DelegateCommand(GoToDaySummary)); }
        }

        [ImportingConstructor]
        public DataHostViewModel(ExportFactory<AppDetailsViewModel> appDetailsVMFactory,
                                 ExportFactory<ScreenshotsViewModel> screenshotsVMFactory,
                                 ExportFactory<DaySummaryViewModel> daySummaryVMFactory)
        {
            RegisterChild(() =>
            {
                using (var context = appDetailsVMFactory.CreateExport())
                {
                    return context.Value;
                }
            });
            RegisterChild(() =>
            {
                using (var context = screenshotsVMFactory.CreateExport())
                {
                    return context.Value;
                }
            });
            RegisterChild(() =>
            {
                using (var context = daySummaryVMFactory.CreateExport())
                {
                    return context.Value;
                }
            });

            SelectedChild = GetChild<AppDetailsViewModel>();
        }


        private void GoToAppDetails()
        {
            SelectedChild = GetChild<AppDetailsViewModel>();
        }


        private void GoToScreenshots()
        {
            SelectedChild = GetChild<ScreenshotsViewModel>();
        }


        private void GoToDaySummary()
        {
            SelectedChild = GetChild<DaySummaryViewModel>();
        }
    }
}

#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AppsTracker.Pages.ViewModels
{
    internal sealed class Statistics_screenshotsViewModel : ViewModelBase, ICommunicator
    {
        private readonly IChartService chartService;

        public override string Title
        {
            get { return "SCREENSHOTS"; }
        }

        public object SelectedItem { get; set; }

        private ICommand returnFromDetailedViewCommand;
        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }

        private ScreenshotModel screenshotModel;
        public ScreenshotModel ScreenshotModel
        {
            get { return screenshotModel; }
            set
            {
                screenshotModel = value;
                PropertyChanging("ScreenshotModel");
                if (screenshotModel != null)
                    dailyScreenshotsList.Reload();
            }
        }

        private readonly AsyncProperty<IEnumerable<ScreenshotModel>> screenshotList;
        public AsyncProperty<IEnumerable<ScreenshotModel>> ScreenshotList
        {
            get { return screenshotList; }
        }

        private readonly AsyncProperty<IEnumerable<DailyScreenshotModel>> dailyScreenshotsList;
        public AsyncProperty<IEnumerable<DailyScreenshotModel>> DailyScreenshotsList
        {
            get { return dailyScreenshotsList; }
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public Statistics_screenshotsViewModel()
        {
            chartService = ServiceFactory.Get<IChartService>();

            screenshotList = new AsyncProperty<IEnumerable<ScreenshotModel>>(GetContent, this);
            dailyScreenshotsList = new AsyncProperty<IEnumerable<DailyScreenshotModel>>(GetSubContent, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }

        public void ReloadAll()
        {
            screenshotList.Reload();
            dailyScreenshotsList.Reload();
        }

        private IEnumerable<ScreenshotModel> GetContent()
        {
            return chartService.GetScreenshots(Globals.SelectedUserID, Globals.Date1, Globals.Date2);
        }

        private IEnumerable<DailyScreenshotModel> GetSubContent()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;

            return chartService.GetScreenshotsByApp(Globals.SelectedUserID, model.AppName, Globals.Date1, Globals.Date2);
        }

        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}

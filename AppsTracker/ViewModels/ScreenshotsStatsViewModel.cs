#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class ScreenshotsStatsViewModel : ViewModelBase
    {
        private readonly IStatsService statsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;


        public override string Title
        {
            get { return "SCREENSHOTS"; }
        }


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
                SetPropertyValue(ref screenshotModel, value);
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



        [ImportingConstructor]
        public ScreenshotsStatsViewModel(IStatsService statsService, 
                                         ITrackingService trackingService, 
                                         IMediator mediator)
        {
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            screenshotList = new AsyncProperty<IEnumerable<ScreenshotModel>>(GetScreenshots, this);
            dailyScreenshotsList = new AsyncProperty<IEnumerable<DailyScreenshotModel>>(GetDailyScreenshots, this);

            this.mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }


        private IEnumerable<ScreenshotModel> GetScreenshots()
        {
            return statsService.GetScreenshots(trackingService.SelectedUserID, trackingService.DateFrom, trackingService.DateTo);
        }


        private IEnumerable<DailyScreenshotModel> GetDailyScreenshots()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;

            return statsService.GetScreenshotsByApp(trackingService.SelectedUserID, model.AppName, trackingService.DateFrom, trackingService.DateTo);
        }

        private void ReloadAll()
        {
            screenshotList.Reload();
            dailyScreenshotsList.Reload();
        }


        private void ReturnFromDetailedView()
        {
            ScreenshotModel = null;
        }
    }
}

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
using System.Linq;
using System.Windows.Input;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;
using AppsTracker.Tracking;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class ScreenshotsStatsViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
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
        public ScreenshotsStatsViewModel(IDataService dataService,
                                         ITrackingService trackingService,
                                         IMediator mediator)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            screenshotList = new TaskRunner<IEnumerable<ScreenshotModel>>(GetScreenshots, this);
            dailyScreenshotsList = new TaskRunner<IEnumerable<DailyScreenshotModel>>(GetDailyScreenshots, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private IEnumerable<ScreenshotModel> GetScreenshots()
        {
            var screenshots = dataService.GetFiltered<Screenshot>(
                                                s => s.Log.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && s.Date >= trackingService.DateFrom
                                                && s.Date <= trackingService.DateTo,
                                                s => s.Log.Window.Application,
                                                s => s.Log.Window.Application.User);

            var screenshotModels = screenshots
                                            .GroupBy(s => s.Log.Window.Application.Name)
                                            .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() });

            return screenshotModels;
        }


        private IEnumerable<DailyScreenshotModel> GetDailyScreenshots()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;

            var screenshots = dataService.GetFiltered<Screenshot>(
                                                s => s.Log.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && s.Date >= trackingService.DateFrom
                                                && s.Date <= trackingService.DateTo
                                                && s.Log.Window.Application.Name == model.AppName,
                                                s => s.Log.Window.Application,
                                                s => s.Log.Window.Application.User);

            var grouped = screenshots.GroupBy(s => new
                                        {
                                            year = s.Date.Year,
                                            month = s.Date.Month,
                                            day = s.Date.Day
                                        })
                                      .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            return grouped.Select(g => new DailyScreenshotModel()
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                Count = g.Count()
            });
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

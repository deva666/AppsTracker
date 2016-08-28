#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using AppsTracker.Common.Communication;
using AppsTracker.Domain;
using AppsTracker.Domain.Screenshots;
using AppsTracker.MVVM;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class ScreenshotsStatsViewModel : ViewModelBase
    {
        private readonly IUseCase<ScreenshotOverview> screenshotModelUseCase;
        private readonly IUseCase<String, DailyScreenshotModel> dailyScreenshotModelUseCase;
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


        private ScreenshotOverview screenshotModel;

        public ScreenshotOverview ScreenshotModel
        {
            get { return screenshotModel; }
            set
            {
                SetPropertyValue(ref screenshotModel, value);
                if (screenshotModel != null)
                    dailyScreenshotsList.Reload();
            }
        }


        private readonly AsyncProperty<IEnumerable<ScreenshotOverview>> screenshotList;

        public AsyncProperty<IEnumerable<ScreenshotOverview>> ScreenshotList
        {
            get { return screenshotList; }
        }


        private readonly AsyncProperty<IEnumerable<DailyScreenshotModel>> dailyScreenshotsList;

        public AsyncProperty<IEnumerable<DailyScreenshotModel>> DailyScreenshotsList
        {
            get { return dailyScreenshotsList; }
        }



        [ImportingConstructor]
        public ScreenshotsStatsViewModel(IUseCase<ScreenshotOverview> screenshotModelUseCase,
                                         IUseCase<String, DailyScreenshotModel> dailyScreenshotModelUseCase,
                                         IMediator mediator)
        {
            this.screenshotModelUseCase = screenshotModelUseCase;
            this.dailyScreenshotModelUseCase = dailyScreenshotModelUseCase;
            this.mediator = mediator;

            screenshotList = new TaskRunner<IEnumerable<ScreenshotOverview>>(screenshotModelUseCase.Get, this);
            dailyScreenshotsList = new TaskRunner<IEnumerable<DailyScreenshotModel>>(GetDailyScreenshots, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(ReloadAll));
        }


        private IEnumerable<DailyScreenshotModel> GetDailyScreenshots()
        {
            var model = ScreenshotModel;
            if (model == null)
                return null;

            return dailyScreenshotModelUseCase.Get(model.AppName);
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

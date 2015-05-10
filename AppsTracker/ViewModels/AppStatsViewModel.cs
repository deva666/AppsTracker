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
    public sealed class AppStatsViewModel : ViewModelBase
    {
        private readonly IStatsService statsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;

        public override string Title
        {
            get { return "APPS"; }
        }


        private AppDuration selectedApp;

        public AppDuration SelectedApp
        {
            get { return selectedApp; }
            set
            {
                SetPropertyValue(ref selectedApp, value);
                dailyAppList.Reload();
            }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDuration>> appsList;

        public AsyncProperty<IEnumerable<AppDuration>> AppsList
        {
            get { return appsList; }
        }


        private readonly AsyncProperty<IEnumerable<DailyAppDuration>> dailyAppList;

        public AsyncProperty<IEnumerable<DailyAppDuration>> DailyAppList
        {
            get { return dailyAppList; }
        }


        private ICommand returnFromDetailedViewCommand;

        public ICommand ReturnFromDetailedViewCommand
        {
            get { return returnFromDetailedViewCommand ?? (returnFromDetailedViewCommand = new DelegateCommand(ReturnFromDetailedView)); }
        }


        [ImportingConstructor]
        public AppStatsViewModel(IStatsService statsService,
                                 ITrackingService trackingService,
                                 IMediator mediator)
        {
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            appsList = new AsyncProperty<IEnumerable<AppDuration>>(GetApps, this);
            dailyAppList = new AsyncProperty<IEnumerable<DailyAppDuration>>(GetDailyApp, this);

            this.mediator.Register(MediatorMessages.RefreshLogs, new Action(ReloadAll));
        }


        private void ReloadAll()
        {
            appsList.Reload();
            dailyAppList.Reload();
        }


        private IEnumerable<AppDuration> GetApps()
        {
            return statsService.GetAppsDuration(trackingService.SelectedUserID, trackingService.DateFrom, trackingService.DateTo);
        }


        private IEnumerable<DailyAppDuration> GetDailyApp()
        {
            var app = selectedApp;
            if (app == null)
                return null;

            return statsService.GetAppDurationByDate(trackingService.SelectedUserID, app.Name, trackingService.DateFrom, trackingService.DateTo);
        }


        private void ReturnFromDetailedView()
        {
            SelectedApp = null;
        }

    }
}

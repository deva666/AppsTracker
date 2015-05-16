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
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;

namespace AppsTracker.ViewModels
{
    [Export, PartCreationPolicy(CreationPolicy.Any)]
    public sealed class DailyAppUsageViewModel : ViewModelBase
    {
        private readonly IStatsService statsService;
        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;


        public override string Title
        {
            get { return "DAILY APP USAGE"; }
        }


        public object SelectedItem { get; set; }


        private readonly AsyncProperty<IEnumerable<AppDurationOverview>> appsList;

        public AsyncProperty<IEnumerable<AppDurationOverview>> AppsList
        {
            get { return appsList; }
        }


        [ImportingConstructor]
        public DailyAppUsageViewModel(IStatsService statsService,
                                      ITrackingService trackingService,
                                      IMediator mediator)
        {
            this.statsService = statsService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            appsList = new AsyncProperty<IEnumerable<AppDurationOverview>>(GetApps, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(appsList.Reload));
        }


        private IEnumerable<AppDurationOverview> GetApps()
        {
            return statsService.GetAppsUsageSeries(trackingService.SelectedUserID, trackingService.DateFrom, trackingService.DateTo);
        }
    }
}

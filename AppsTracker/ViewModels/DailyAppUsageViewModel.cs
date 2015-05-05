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
        private readonly ILoggingService loggingService;
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
                                      ILoggingService loggingService,
                                      IMediator mediator)
        {
            this.statsService = statsService;
            this.loggingService = loggingService;
            this.mediator = mediator;

            appsList = new AsyncProperty<IEnumerable<AppDurationOverview>>(GetApps, this);

            this.mediator.Register(MediatorMessages.RefreshLogs, new Action(appsList.Reload));
        }


        private IEnumerable<AppDurationOverview> GetApps()
        {
            return statsService.GetAppsUsageSeries(loggingService.SelectedUserID, loggingService.DateFrom, loggingService.DateTo);
        }
    }
}

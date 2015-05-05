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
    public sealed class DailyAppUsageViewModel : ViewModelBase, ICommunicator
    {
        private readonly IStatsService statsService;
        private readonly ILoggingService loggingService;


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


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        [ImportingConstructor]
        public DailyAppUsageViewModel(IStatsService statsService, ILoggingService loggingService)
        {
            this.statsService = statsService;
            this.loggingService = loggingService;

            appsList = new AsyncProperty<IEnumerable<AppDurationOverview>>(GetApps, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(appsList.Reload));
        }


        private IEnumerable<AppDurationOverview> GetApps()
        {
            return statsService.GetAppsUsageSeries(loggingService.SelectedUserID, loggingService.DateFrom, loggingService.DateTo);
        }
    }
}

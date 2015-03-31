#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.ServiceLocation;

namespace AppsTracker.ViewModels
{
    internal sealed class DailyAppUsageViewModel : ViewModelBase, ICommunicator
    {
        private readonly IStatsService statsService;


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
            get { return ServiceLocation.Mediator.Instance; }
        }


        public DailyAppUsageViewModel()
        {
            statsService = serviceResolver.Resolve<IStatsService>();

            appsList = new AsyncProperty<IEnumerable<AppDurationOverview>>(GetApps, this);

            Mediator.Register(MediatorMessages.RefreshLogs, new Action(appsList.Reload));
        }


        private IEnumerable<AppDurationOverview> GetApps()
        {
            return statsService.GetAppsUsageSeries(Globals.SelectedUserID, Globals.DateFrom, Globals.DateTo);
        }
    }
}

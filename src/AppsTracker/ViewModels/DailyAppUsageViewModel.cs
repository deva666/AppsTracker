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
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Data.Service;
using AppsTracker.Common.Communication;
using AppsTracker.Tracking;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class DailyAppUsageViewModel : ViewModelBase
    {
        private readonly IDataService dataService;
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
        public DailyAppUsageViewModel(IDataService dataService,
                                      ITrackingService trackingService,
                                      IMediator mediator)
        {
            this.dataService = dataService;
            this.trackingService = trackingService;
            this.mediator = mediator;

            appsList = new TaskRunner<IEnumerable<AppDurationOverview>>(GetApps, this);

            this.mediator.Register(MediatorMessages.REFRESH_LOGS, new Action(appsList.Reload));
        }


        private IEnumerable<AppDurationOverview> GetApps()
        {
            var logs = dataService.GetFiltered<Log>(
                                          l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                          && l.DateCreated >= trackingService.DateFrom
                                          && l.DateCreated <= trackingService.DateTo,
                                          l => l.Window.Application,
                                          l => l.Window.Application.User);

            var logsGroupedByDay = logs.OrderBy(l => l.DateCreated)
                                  .GroupBy(l => new
                                    {
                                        year = l.DateCreated.Year,
                                        month = l.DateCreated.Month,
                                        day = l.DateCreated.Day,
                                        name = l.Window.Application.Name
                                    });

            var dailyDurations = logsGroupedByDay.Select(g => new
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                AppName = g.Key.name,
                Duration = g.Sum(l => l.Duration)
            });

            List<AppDuration> dailyDurationCollection;
            var dailyDurationSeries = new List<AppDurationOverview>();

            foreach (var app in dailyDurations)
            {
                if (app.Duration > 0)
                {
                    if (!dailyDurationSeries.Exists(d => d.Date == app.Date.ToShortDateString()))
                    {
                        dailyDurationCollection = new List<AppDuration>();
                        dailyDurationCollection.Add(new AppDuration() { Name = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                        dailyDurationSeries.Add(new AppDurationOverview() { Date = app.Date.ToShortDateString(), AppCollection = dailyDurationCollection });
                    }
                    else
                    {
                        dailyDurationSeries.First(d => d.Date == app.Date.ToShortDateString())
                            .AppCollection.Add(new AppDuration() { Name = app.AppName, Duration = Math.Round(new TimeSpan(app.Duration).TotalHours, 1) });
                    }
                }
            }

            foreach (var item in dailyDurationSeries)
                item.AppCollection.Sort((x, y) => x.Duration.CompareTo(y.Duration));

            return dailyDurationSeries;
        }
    }
}

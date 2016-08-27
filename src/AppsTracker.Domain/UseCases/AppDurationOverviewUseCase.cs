using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.UseCases
{
    [Export(typeof(IUseCase<AppDurationOverview>))]
    public sealed class AppDurationOverviewUseCase : IUseCase<AppDurationOverview>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppDurationOverviewUseCase(IRepository repository,
                                          ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppDurationOverview> Get()
        {
            var logs = repository.GetFiltered<Log>(
                                          l => l.Window.Application.User.UserID ==  trackingService.SelectedUserID
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

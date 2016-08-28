using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<String, DailyAppDuration>))]
    public sealed class DailyAppDurationUseCase : IUseCase<String, DailyAppDuration>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public DailyAppDurationUseCase(IRepository repository,
                                       ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<DailyAppDuration> Get(String appName)
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.Name == appName
                                               && l.Window.Application.User.UserID == trackingService.SelectedUserID
                                               && l.DateCreated >= trackingService.DateFrom
                                               && l.DateCreated <= trackingService.DateTo);

            var grouped = logs.GroupBy(l => new
            {
                year = l.DateCreated.Year,
                month = l.DateCreated.Month,
                day = l.DateCreated.Day
            })
                                          .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            return grouped.Select(g => new DailyAppDuration
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
            });

        }
    }
}

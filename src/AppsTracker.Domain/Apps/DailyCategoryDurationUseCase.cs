using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<String, DailyCategoryDuration>))]
    public sealed class DailyCategoryDurationUseCase : IUseCase<String, DailyCategoryDuration>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public DailyCategoryDurationUseCase(IRepository repository,
                                            ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<DailyCategoryDuration> Get(String categoryName)
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.Categories.Any(c => c.Name == categoryName)
                                              && l.Window.Application.UserID == trackingService.SelectedUserID
                                              && l.DateCreated >= trackingService.DateFrom
                                              && l.DateCreated <= trackingService.DateTo);

            var grouped = logs.GroupBy(l => new
            {
                year = l.DateCreated.Year,
                month = l.DateCreated.Month,
                day = l.DateCreated.Day
            });

            return grouped.Select(g => new DailyCategoryDuration()
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                TotalTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 2)
            });
        }
    }
}

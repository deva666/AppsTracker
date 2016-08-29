using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<AppDuration>))]
    public sealed class AppDurationUseCase : IUseCase<AppDuration>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppDurationUseCase(IRepository repository,
                                  ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppDuration> Get()
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.ID == trackingService.SelectedUserID
                                        && l.DateCreated >= trackingService.DateFrom
                                        && l.DateCreated <= trackingService.DateTo,
                                        l => l.Window.Application,
                                        l => l.Window.Application.User);

            var grouped = logs.GroupBy(l => l.Window.Application.Name);

            return grouped.Select(g => new AppDuration()
            {
                Name = g.Key,
                Duration = Math.Round(new TimeSpan(g.Sum(l => l.Duration)).TotalHours, 1)
            });
        }
    }
}

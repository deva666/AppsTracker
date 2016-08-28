using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Windows
{
    [Export(typeof(IUseCase<String, IEnumerable<DateTime>, WindowSummary>))]
    public sealed class WindowSummaryStatsUseCase : IUseCase<String, IEnumerable<DateTime>, WindowSummary>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public WindowSummaryStatsUseCase(IRepository repository,
                                         ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<WindowSummary> Get(String appName, IEnumerable<DateTime> selectedDates)
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                   && l.Window.Application.Name == appName,
                                                   l => l.Window);

            var logsInSelectedDateRange = logs.Where(l => selectedDates.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)));

            double totalDuration = logsInSelectedDateRange.Sum(l => l.Duration);

            var result = (from l in logsInSelectedDateRange
                          group l by l.Window.Title into grp
                          select grp)
                          .Select(g => new WindowSummary
                          {
                              Title = g.Key,
                              Usage = (g.Sum(l => l.Duration) / totalDuration),
                              Duration = g.Sum(l => l.Duration)
                          })
                          .OrderByDescending(t => t.Duration)
                          .ToList();

            return result;
        }
    }
}

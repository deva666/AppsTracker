using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Windows
{
    [Export(typeof(IUseCase<String, DateTime, WindowSummary>))]
    public sealed class WindowSummaryUseCase : IUseCase<String, DateTime, WindowSummary>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public WindowSummaryUseCase(IRepository repository,
                                    ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<WindowSummary> Get(String appName, DateTime selectedDate)
        {
            var nextDay = selectedDate.AddDays(1);

            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                         && l.DateCreated >= selectedDate
                                                         && l.DateCreated <= nextDay
                                                         && l.Window.Application.Name == appName,
                                                    l => l.Window);

            double totalDuration = logs.Sum(l => l.Duration);

            return logs.GroupBy(l => l.Window.Title)
                                  .Select(g => new WindowSummary
                                  {
                                      Title = g.Key,
                                      Usage = (g.Sum(l => l.Duration) / totalDuration),
                                      Duration = g.Sum(l => l.Duration)
                                  })
                                  .OrderByDescending(t => t.Duration)
                                  .ToList();
        }
    }
}

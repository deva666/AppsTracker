using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<String, Int32, AppSummary>))]
    public sealed class AppSummaryStatsUseCase : IUseCase<String, Int32, AppSummary>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppSummaryStatsUseCase(IRepository repository,
                                      ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppSummary> Get(String appName, Int32 appId)
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && l.DateCreated >= trackingService.DateFrom
                                                && l.DateCreated <= trackingService.DateTo,
                                                l => l.Window.Application);

            var totalDuration = (from l in logs
                                 group l by new
                                 {
                                     year = l.DateCreated.Year,
                                     month = l.DateCreated.Month,
                                     day = l.DateCreated.Day
                                 } into grp
                                 select grp)
                                .Select(g => new
                                {
                                    Date = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                                    Duration = (Double)g.Sum(l => l.Duration)
                                });


            var result = (from l in logs
                          where l.Window.Application.ApplicationID == appId
                          group l by new
                          {
                              year = l.DateCreated.Year,
                              month = l.DateCreated.Month,
                              day = l.DateCreated.Day,
                              name = l.Window.Application.Name
                          } into grp
                          select grp)
                          .Select(g => new AppSummary
                          {
                              AppName = g.Key.name,
                              Date = new DateTime(g.Key.year, g.Key.month, g.Key.day)
                                   .ToShortDateString()
                                   + " " + new DateTime(g.Key.year, g.Key.month, g.Key.day)
                                   .DayOfWeek.ToString(),
                              DateTime = new DateTime(g.Key.year, g.Key.month, g.Key.day),
                              Usage = g.Sum(l => l.Duration) / totalDuration
                                   .First(t => t.Date == new DateTime(g.Key.year, g.Key.month, g.Key.day)).Duration,
                              Duration = g.Sum(l => l.Duration)
                          })
                          .OrderByDescending(t => t.DateTime)
                          .ToList();

            var requestedApp = result.Where(a => a.AppName == appName).FirstOrDefault();

            if (requestedApp != null)
                requestedApp.IsSelected = true;

            return result;
        }
    }
}

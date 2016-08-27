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
    [Export(typeof(IUseCase<DateTime, AppSummary>))]
    public sealed class AppSummaryUseCase : IUseCase<DateTime, AppSummary>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppSummaryUseCase(IRepository repository,
                                 ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<AppSummary> Get(DateTime selectedDate)
        {
            var dateTo = selectedDate.AddDays(1);
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                            && l.DateCreated >= selectedDate
                                            && l.DateCreated <= dateTo,
                                            l => l.Window.Application);

            Double totalDuration = (from l in logs
                                    select (Double?)l.Duration).Sum() ?? 0;

            var appSummaries = (from l in logs
                                group l by l.Window.Application.Name into grp
                                select grp)
                                 .Select(g => new AppSummary
                                 {
                                     AppName = g.Key,
                                     Date = selectedDate.ToShortDateString(),
                                     Usage = (g.Sum(l => l.Duration) / totalDuration),
                                     Duration = g.Sum(l => l.Duration)
                                 })
                                 .OrderByDescending(t => t.Duration);

            var first = appSummaries.FirstOrDefault();
            if (first != null)
                first.IsSelected = true;

            return appSummaries;
        }
    }
}

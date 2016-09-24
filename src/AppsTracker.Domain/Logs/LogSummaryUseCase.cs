using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;
using AppsTracker.Domain.Util;

namespace AppsTracker.Domain.Logs
{
    [Export(typeof(IUseCase<DateTime, LogSummary>))]
    public sealed class LogSummaryUseCase : IUseCase<DateTime, LogSummary>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public LogSummaryUseCase(IRepository repository,
                                 ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<LogSummary> Get(DateTime selectedDate)
        {
            var dateTo = selectedDate.AddDays(1);

            var logsTask = repository.GetFilteredAsync<Log>(l => l.Window.Application.User.ID == trackingService.SelectedUserID
                               && l.DateCreated >= selectedDate
                               && l.DateCreated <= dateTo,
                               l => l.Window.Application);

            var usagesTask = repository.GetFilteredAsync<Usage>(u => u.User.ID == trackingService.SelectedUserID
                                     && u.UsageStart >= selectedDate
                                     && u.UsageEnd <= dateTo
                                     && u.UsageType != UsageTypes.Login);

            Task.WaitAll(logsTask, usagesTask);

            var logs = logsTask.Result;
            var usages = usagesTask.Result;

            var logModels = logs.Select(l => new LogSummary()
            {
                DateCreated = l.DateCreated.ToString("HH:mm:ss"),
                DateEnded = l.DateEnded.ToString("HH:mm:ss"),
                Duration = l.Duration,
                Name = l.Window.Application.Name,
                Title = l.Window.Title
            });

            var usageModels = usages.Select(u => new LogSummary()
            {
                DateCreated = u.UsageStart.ToString("HH:mm:ss"),
                DateEnded = u.UsageEnd.ToString("HH:mm:ss"),
                Duration = u.Duration.Ticks,
                Name = u.UsageType.ToExtendedString(),
                Title = "*********",
                IsRequested = true
            });

            return logModels.Union(usageModels).OrderBy(d => d.DateCreated).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Windows
{
    [Export(typeof(IUseCase<String, IEnumerable<String>, IEnumerable<DateTime>, WindowDurationOverview>))]
    public sealed class WindowDurationOverviewUseCase : IUseCase<String, IEnumerable<String>, IEnumerable<DateTime>, WindowDurationOverview>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public WindowDurationOverviewUseCase(IRepository repository,
                                             ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<WindowDurationOverview> Get(String appName,
                                                       IEnumerable<String> selectedWindows,
                                                       IEnumerable<DateTime> selectedDates)
        {
            var logs = repository.GetFiltered<Log>(l => l.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && l.Window.Application.Name == appName,
                                                l => l.Window);

            var filteredLogs = logs.Where(l => selectedDates.Any(d => l.DateCreated >= d && l.DateCreated <= d.AddDays(1d)) && selectedWindows.Contains(l.Window.Title));

            var result = new List<WindowDurationOverview>();

            var logsGroupedByDay = from l in filteredLogs
                                   group l by new { year = l.DateCreated.Year, month = l.DateCreated.Month, day = l.DateCreated.Day } into grp
                                   select grp;

            foreach (var grp in logsGroupedByDay)
            {
                var logsGroupedByWindowTitle = grp.GroupBy(g => g.Window.Title);
                var date = new DateTime(grp.Key.year, grp.Key.month, grp.Key.day);
                var series = new WindowDurationOverview()
                {
                    Date = date.ToShortDateString() + " " + date.DayOfWeek.ToString()
                };
                var modelList = new List<WindowDuration>();
                foreach (var grp2 in logsGroupedByWindowTitle)
                {
                    WindowDuration model = new WindowDuration() { Title = grp2.Key, Duration = Math.Round(new TimeSpan(grp2.Sum(l => l.Duration)).TotalMinutes, 2) };
                    modelList.Add(model);
                }
                series.DurationCollection = modelList;
                result.Add(series);
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCase<CategoryDuration>))]
    public sealed class CategoryDurationStatsUseCase : IUseCase<CategoryDuration>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public CategoryDurationStatsUseCase(IRepository repository,
                                            ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<CategoryDuration> Get()
        {
            var dateTo = trackingService.DateFrom.AddDays(1);
            var categoryModels = new List<CategoryDuration>();

            var categories = repository.GetFiltered<AppCategory>(c => c.Applications.Count > 0
                        && c.Applications.Where(a => a.UserID == trackingService.SelectedUserID)
                                         .Any()
                        && c.Applications.SelectMany(a => a.Windows)
                                        .SelectMany(w => w.Logs)
                                        .Where(l => l.DateCreated >= trackingService.DateFrom)
                                        .Any()
                        && c.Applications.SelectMany(a => a.Windows)
                                        .SelectMany(w => w.Logs)
                                        .Where(l => l.DateCreated <= dateTo)
                                        .Any(),
                       c => c.Applications,
                       c => c.Applications.Select(a => a.Windows),
                       c => c.Applications.Select(a => a.Windows.Select(w => w.Logs)));

            foreach (var cat in categories)
            {
                var totalDuration = cat.Applications
                                       .SelectMany(a => a.Windows)
                                       .SelectMany(w => w.Logs)
                                       .Where(l => l.DateCreated >= trackingService.DateFrom
                                           && l.DateCreated <= dateTo)
                                       .Sum(l => l.Duration);

                categoryModels.Add(new CategoryDuration()
                {
                    Name = cat.Name,
                    TotalTime = Math.Round(new TimeSpan(totalDuration).TotalHours, 2)
                });
            }

            return categoryModels;
        }
    }
}

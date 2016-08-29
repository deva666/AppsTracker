using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Screenshots
{
    [Export(typeof(IUseCase<String, DailyScreenshotModel>))]
    public sealed class DailyScreenshotModelUseCase : IUseCase<String, DailyScreenshotModel>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public DailyScreenshotModelUseCase(IRepository repository,
                                           ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<DailyScreenshotModel> Get(String appName)
        {
            var screenshots = repository.GetFiltered<Screenshot>(
                                                s => s.Log.Window.Application.User.ID == trackingService.SelectedUserID
                                                && s.Date >= trackingService.DateFrom
                                                && s.Date <= trackingService.DateTo
                                                && s.Log.Window.Application.Name == appName,
                                                s => s.Log.Window.Application,
                                                s => s.Log.Window.Application.User);

            var grouped = screenshots.GroupBy(s => new
            {
                year = s.Date.Year,
                month = s.Date.Month,
                day = s.Date.Day
            })
                                      .OrderBy(g => new DateTime(g.Key.year, g.Key.month, g.Key.day));

            return grouped.Select(g => new DailyScreenshotModel()
            {
                Date = new DateTime(g.Key.year, g.Key.month, g.Key.day).ToShortDateString(),
                Count = g.Count()
            });
        }
    }
}

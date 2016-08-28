using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Screenshots
{
    [Export(typeof(IUseCase<ScreenshotModel>))]
    public sealed class ScreenshotModelUseCase : IUseCase<ScreenshotModel>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public ScreenshotModelUseCase(IRepository repository,
                                      ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<ScreenshotModel> Get()
        {
            var screenshots = repository.GetFiltered<Screenshot>(
                                                s => s.Log.Window.Application.User.UserID == trackingService.SelectedUserID
                                                && s.Date >= trackingService.DateFrom
                                                && s.Date <= trackingService.DateTo,
                                                s => s.Log.Window.Application,
                                                s => s.Log.Window.Application.User);

            var screenshotModels = screenshots
                                            .GroupBy(s => s.Log.Window.Application.Name)
                                            .Select(g => new ScreenshotModel() { AppName = g.Key, Count = g.Count() });

            return screenshotModels;
        }
    }
}

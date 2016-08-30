using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Screenshots
{
    [Export(typeof(IScreenshotService))]
    public sealed class ScreenshotsService : IScreenshotService
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public ScreenshotsService(IRepository repository,
                                  ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public async Task<IEnumerable<ScreenshotModel>> GetAsync()
        {
            return (await repository.GetFilteredAsync<Log>(l => l.Screenshots.Count > 0
                                                && l.DateCreated >= trackingService.DateFrom
                                                && l.DateCreated <= trackingService.DateTo
                                                && l.Window.Application.UserID == trackingService.SelectedUserID
                                                , l => l.Screenshots
                                                , l => l.Window.Application)
                    ).Select(l => new ScreenshotModel(l));
        }

        public async Task DeleteScreenshotsAsync(IEnumerable<Image> screenshots)
        {
            await repository.DeleteByIdsAsync<Screenshot>(screenshots.Select(i => i.ScreenshotId));
        }
    }
}

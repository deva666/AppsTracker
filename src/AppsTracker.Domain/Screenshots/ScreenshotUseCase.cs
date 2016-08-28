using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Logs;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Domain.Screenshots
{
    [Export(typeof(IUseCaseAsync<LogModel>))]
    public sealed class ScreenshotUseCase : IUseCaseAsync<LogModel>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public ScreenshotUseCase(IRepository repository,
                                 ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public async Task<IEnumerable<LogModel>> GetAsync()
        {
            return (await repository.GetFilteredAsync<Log>(l => l.Screenshots.Count > 0
                                                && l.DateCreated >= trackingService.DateFrom
                                                && l.DateCreated <= trackingService.DateTo
                                                && l.Window.Application.UserID == trackingService.SelectedUserID
                                                , l => l.Screenshots
                                                , l => l.Window.Application)
                    ).Select(l => new LogModel(l));
        }
    }
}

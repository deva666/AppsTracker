using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Apps
{
    [Export(typeof(IUseCaseAsync<Aplication>))]
    public sealed class AppStatsUseCaseAsync : IUseCaseAsync<Aplication>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public AppStatsUseCaseAsync(IRepository repository,
                                    ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public async Task<IEnumerable<Aplication>> GetAsync()
        {
            return (await repository.GetFilteredAsync<Aplication>(a => a.User.UserID == trackingService.SelectedUserID
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= trackingService.DateFrom).Any()
                                                                && a.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated <= trackingService.DateTo).Any()))
                                                           .Distinct();
        }
    }
}

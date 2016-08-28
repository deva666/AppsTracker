using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking;

namespace AppsTracker.Domain.Usages
{
    [Export(typeof(IUseCase<UserLoggedTime>))]
    public sealed class UserLoggedTimeUseCase : IUseCase<UserLoggedTime>
    {
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;

        [ImportingConstructor]
        public UserLoggedTimeUseCase(IRepository repository,
                                     ITrackingService trackingService)
        {
            this.repository = repository;
            this.trackingService = trackingService;
        }

        public IEnumerable<UserLoggedTime> Get()
        {
            var logins = repository.GetFiltered<Usage>(u => u.UsageStart >= trackingService.DateFrom
                                                    && u.UsageStart <= trackingService.DateTo
                                                    && u.UsageType == UsageTypes.Login,
                                                    u => u.User);

            return logins.GroupBy(u => u.User.Name)
                            .Select(g => new UserLoggedTime
                            {
                                Username = g.Key,
                                LoggedInTime = Math.Round(new TimeSpan(g.Sum(l => l.Duration.Ticks)).TotalHours, 1)
                            });
        }
    }
}

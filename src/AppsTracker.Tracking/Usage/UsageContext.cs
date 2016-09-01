using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Domain.Tracking;

namespace AppsTracker.Tracking.Usage
{
    class UsageContext : ITrackingModule, IObserver<UsageEvent>
    {
        private readonly IEnumerable<IUsageNotifier> notifiers;
        private readonly IRepository repository;
        private readonly ITrackingService trackingService;


        private bool isTrackingEnabled;

        private readonly IDictionary<UsageTypes, Data.Models.Usage> typesToUsagesMap =
            new Dictionary<UsageTypes, Data.Models.Usage>();

        public int InitializationOrder
        {
            get { return 0; }
        }

        public UsageContext()
        {
            foreach (var n in notifiers)
            {
                n.UsageObservable
                    .Where(u => isTrackingEnabled)
                    .Subscribe(this);
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public async void OnNext(UsageEvent value)
        {
            if (value.Action == UsageAction.Start)
            {
                var newUsage = new Data.Models.Usage(trackingService.UserID, value.Type);
                typesToUsagesMap.Add(value.Type, newUsage);
            }
            else
            {
                Data.Models.Usage usage;
                if (typesToUsagesMap.TryGetValue(value.Type, out usage))
                {
                    usage.IsCurrent = false;
                    usage.UsageEnd = DateTime.Now;
                    await repository.SaveNewEntityAsync(usage);
                }
                else
                {
#if DEBUG
                    throw new InvalidOperationException("usage type not started");
#endif
                }
            }
        }

        public void SettingsChanged(Setting settings)
        {
            throw new NotImplementedException();
        }

        public void Initialize(Setting settings)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
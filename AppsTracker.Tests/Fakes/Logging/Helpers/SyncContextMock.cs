using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tests.Fakes.Logging.Helpers
{
    [Export(typeof(ISyncContext))]
    class SyncContextMock : ISyncContext
    {
        public System.Threading.SynchronizationContext Context
        {
            get;
            set;
        }

        public void Invoke(System.Threading.SendOrPostCallback method, object state = null)
        {
            if (Context == null)
                throw new InvalidOperationException("Context is null");

            Context.Post(method, state);
        }

        public void Invoke(Action action)
        {
            if (Context == null)
                throw new InvalidOperationException("Context is null");

            Context.Post(s => action(), null);
        }
    }
}

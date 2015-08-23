using System;
using System.ComponentModel.Composition;
using AppsTracker.Communication;

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

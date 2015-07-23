using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace AppsTracker.Communication
{
    [Export(typeof(ISyncContext))]
    internal sealed class SyncContext : ISyncContext
    {
        private SynchronizationContext context;

        public SynchronizationContext Context
        {
            get { return context; }
            set { context = value; }
        }

        public SyncContext()
        {
            context = SynchronizationContext.Current;
        }

        public void Invoke(SendOrPostCallback method, object state = null)
        {
            if (context == null)
                throw new InvalidOperationException("Context not initialized");

            context.Post(method, state);
        }


        public void Invoke(Action action)
        {
            if (context == null)
                throw new InvalidOperationException("Context not initialized");

            context.Post(s => action(), null);
        }
    }
}

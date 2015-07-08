using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tests.Fakes
{
    public sealed class SyncContextFake : ISyncContext
    {
        public System.Threading.SynchronizationContext Context
        {
            get;
            set;
        }

        public void Invoke(System.Threading.SendOrPostCallback method, object state = null)
        {
            method.Invoke(state);
        }

        public void Invoke(Action action)
        {
            action.Invoke();
        }
    }
}
